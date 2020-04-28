using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
// TODO: 0. 필요한 네임스페이스를 추가합니다.
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using EmotionsDetector.Helpers;
using Windows.UI.Popups;
using Windows.ApplicationModel;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x412에 나와 있습니다.

namespace EmotionDetector4Photos
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // TODO: 1. 환경변수에 Face 구독 키와 엔드포인트를 추가하고 Face Client 인스턴스를 만듭니다.
        private const string subscriptionKey = "aab31cca7d7643e99666d7dea976b9a4";
        private const string faceEndpoint = "https://koreacentral.api.cognitive.microsoft.com/";
        private readonly IFaceClient emotionServiceClient = new FaceClient(new ApiKeyServiceClientCredentials(subscriptionKey)) { Endpoint = faceEndpoint };


        // TODO: 2. 이미지 처리를 위한 필드와 얼굴 사각형 스케일링 처리를 위한 필드를 선언한다.
        private BitmapImage imageSource;
        private double xScalingFactor;
        private double yScalingFactor;
        private StorageFile file;
        public MainPage()
        {
            this.InitializeComponent();
        }

        // TODO: 3. FileOpenPicker를 이용해 감정 분석 대상 이미지를 찾아 표시합니다.
        private async void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openDlg = new FileOpenPicker();

            openDlg.ViewMode = PickerViewMode.Thumbnail;
            openDlg.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openDlg.FileTypeFilter.Add(".jpg");
            openDlg.FileTypeFilter.Add(".png");
            openDlg.FileTypeFilter.Add(".bmp");
            openDlg.FileTypeFilter.Add(".gif");
            file = await openDlg.PickSingleFileAsync();

            if (file != null)
            {
                // 이전 얼굴 사각형 모두 지우기
                CanvasFaceDisplay.Children.Clear();

                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                imageSource = new BitmapImage();
                imageSource.SetSource(stream);
                PhotoPreview.Source = imageSource;
            }
            else
            {
                var messageDialog = new MessageDialog("사람 얼굴 사진 넣어라!", "파일 없다.");
                await messageDialog.ShowAsync();
            }
        }

        // TODO: 4. 감지된 얼굴목록을 반환하는 GetEmotion() 메서드
        private async Task<IList<DetectedFace>> GetEmotion(IRandomAccessStream irastream)
        {
            IList<DetectedFace> recognitionResult = null;
            IList<FaceAttributeType> faceAttributes = new FaceAttributeType[] { FaceAttributeType.Emotion };
            try
            {
                Stream stream = irastream.AsStream();
                //2번째 인수는 faceId를 반환하도록 지정
                //3번째 인수는 얼굴 랜드마크를 반환하지 않도록 지정
                recognitionResult = await emotionServiceClient.Face.DetectWithStreamAsync(stream, true, false, faceAttributes);

            }
            catch (Exception ex)
            {
                var messageDialog = new MessageDialog(ex.Message, Package.Current.DisplayName);
                await messageDialog.ShowAsync();
            }
            return recognitionResult;
        }

        // TODO: 5. 얼굴 사각형  표시 조정을 위한 스케일링 계수 계산 메서드 GetScalingFactors()
        private void GetScalingFactors(BitmapImage softwareBitmap)
        {
            xScalingFactor = CanvasFaceDisplay.ActualWidth / softwareBitmap.PixelWidth;
            yScalingFactor = CanvasFaceDisplay.ActualHeight / softwareBitmap.PixelHeight;
        }

        // TODO: 6. 이미지 위에 얼굴 상자를 그리고 감정 설명을 붙이는 메서드 DrawFaceBox()
        private void DrawFaceBox(BitmapImage softwareBitmap, FaceRectangle faceRectangle, string emotionName)
        {
            // 얼굴 사각형 표시를 위해 스케일링 계수 업데이트
            GetScalingFactors(softwareBitmap);

            // 감정에 맞게 색 조정
            var emotionColor = EmotionHelper.GetEmotionColor(emotionName);

            // 얼굴 상자 준비
            var faceBox = EmotionHelper.PrepareFaceBox(faceRectangle, emotionColor, xScalingFactor, yScalingFactor);

            // 감정 설명 준비
            var emotionTextBlock = EmotionHelper.PrepareEmotionTextBlock(faceBox, emotionColor, emotionName);

            // 경계 상자와 감정 설명 표시
            CanvasFaceDisplay.Children.Add(faceBox);
            CanvasFaceDisplay.Children.Add(emotionTextBlock);
        }

        // TODO: 7. 얼굴의 대표 감정을 얻어와 얼굴 사각형 표시하는 메서드 DisplayEmotion()
        private void DisplayEmotion(BitmapImage softwareBitmap, DetectedFace emotion)
        {
            if (emotion != null)
            {
                var emotionName = EmotionHelper.GetTopEmotionName(emotion);

                DrawFaceBox(softwareBitmap, emotion.FaceRectangle, emotionName);
            }
        }

        // TODO: 8. 감정 인식 수행 버튼 이벤트 처리기를 완성합니다. 
        private async void ButtonDetectEmotion_Click(object sender, RoutedEventArgs e)
        {
            //이미지파일 스트림에서 감지된 얼굴 목록을 가져온다.
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            var emotions = await GetEmotion(stream);
            FaceBitmap.Source = imageSource;
            //얼굴마다 감정을 평가한다.
            for (int i = 0; i < emotions.Count; i++)
            {
                DetectedFace emotion = emotions[i];
                DisplayEmotion(imageSource, emotion);
            }
        }
    }
}
