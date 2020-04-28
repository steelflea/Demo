##EmotionHelper.cs
#TODO: 01. 대표 감정을 가져오는 메서드를 작성합니다.
var rankedList = emotion.FaceAttributes.Emotion;
double[] emotionList = { rankedList.Anger, rankedList.Contempt, rankedList.Disgust, rankedList.Fear, rankedList.Happiness, rankedList.Neutral, rankedList.Sadness, rankedList.Surprise };

//Emotion 객체의 대표 감정값을 얻은 뒤, 해당하는 감정 이름을 반환한다.
double emotionResult = emotionList.Max();
if (emotionResult.Equals(rankedList.Anger))
{
    return "Anger";
}
else if (emotionResult.Equals(rankedList.Contempt))
{
    return "Contempt";
}
else if (emotionResult.Equals(rankedList.Disgust))
{
    return "Disgust";
}
else if (emotionResult.Equals(rankedList.Fear))
{
    return "Fear";
}
else if (emotionResult.Equals(rankedList.Happiness))
{
    return "Happiness";
}
else if (emotionResult.Equals(rankedList.Neutral))
{
    return "Neutral";
}
else if (emotionResult.Equals(rankedList.Sadness))
{
    return "Sadness";
}
else
{
    return "Surprise";
}

#TODO: 02. 감정을 색과 연결하는 메서드를 만듭니다.
// 감정 이름을 읽기 위한 더미 객체
var scores = (new FaceAttributes()).Emotion;

switch (emotionName)
{
    case nameof(scores.Happiness):
        return Colors.GreenYellow;

    case nameof(scores.Anger):
        return Colors.Red;

    case nameof(scores.Disgust):
        return Colors.OrangeRed;

    case nameof(scores.Surprise):
        return Colors.HotPink;

    case nameof(scores.Sadness):
        return Colors.Brown;

    case nameof(scores.Fear):
        return Colors.Gray;

    case nameof(scores.Contempt):
        return Colors.Purple;

    default:
    case nameof(scores.Neutral):
        return Colors.White;
}

#TODO: 03. 얼굴 사각형위에 노출된 대표적인 감정의 텍스트 블록 배치를 준비하는 메서드를 만듭니다.
var textBlock = new TextBlock()
{
    Foreground = new SolidColorBrush(emotionColor),
    FontSize = 38,
    Text = emotionName
};

// 텍스트 블록 측정
textBlock.Measure(Size.Empty);

// 오프셋 계산
var xTextBlockOffset = (faceBox.ActualWidth - textBlock.ActualWidth) / 2.0;
var yTextBlockOffset = -textBlock.ActualHeight;

// 음의 수평 오프셋 무시
xTextBlockOffset = Math.Max(0, xTextBlockOffset);

// 텍스트 블록을 얼굴 상자 중심에 오도록 이동
var faceBoxTranslateTransform = faceBox.RenderTransform as TranslateTransform;

textBlock.RenderTransform = new TranslateTransform()
{
    X = faceBoxTranslateTransform.X + xTextBlockOffset,
    Y = faceBoxTranslateTransform.Y + yTextBlockOffset
};

return textBlock;

#TODO: 04. 감성 색을 반영한 얼굴 사각형을 준비하는 메서드를 만듭니다
//얼굴에 바인딩할 사각형 준비
var faceBox = new Rectangle()
{
    Stroke = new SolidColorBrush(emotionColor),
    StrokeThickness = 5,
    Width = faceRectangle.Width * xScalingFactor,
    Height = faceRectangle.Height * yScalingFactor
};

var translateTransform = new TranslateTransform()
{
    X = faceRectangle.Left * xScalingFactor,
    Y = faceRectangle.Top * yScalingFactor
};

faceBox.RenderTransform = translateTransform;

return faceBox;

##MainPage.xml.cs
#TODO: 0. 필요한 네임스페이스를 추가합니다.
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using EmotionsDetector.Helpers;
using Windows.UI.Popups;
using Windows.ApplicationModel;

#TODO: 1. 환경변수에 Face 구독 키와 엔드포인트를 추가하고 Face Client 인스턴스를 만듭니다.
private const string subscriptionKey = "YOUR_KEY";
private const string faceEndpoint = "YOUR_ENDPOINT";
private readonly IFaceClient emotionServiceClient = new FaceClient(new ApiKeyServiceClientCredentials(subscriptionKey)) { Endpoint = faceEndpoint };

#TODO: 2. 이미지 처리를 위한 필드와 얼굴 사각형 스케일링 처리를 위한 필드를 선언한다.
private BitmapImage imageSource;
private double xScalingFactor;
private double yScalingFactor;
private StorageFile file;

#TODO: 3. FileOpenPicker를 이용해 감정 분석 대상 이미지를 찾아 표시합니다.
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

#TODO: 4. 감지된 얼굴목록을 반환하는 GetEmotion() 메서드
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

#TODO: 5. 얼굴 사각형  표시 조정을 위한 스케일링 계수 계산 메서드 GetScalingFactors()
private void GetScalingFactors(BitmapImage softwareBitmap)
{
    xScalingFactor = CanvasFaceDisplay.ActualWidth / softwareBitmap.PixelWidth;
    yScalingFactor = CanvasFaceDisplay.ActualHeight / softwareBitmap.PixelHeight;
}

#TODO: 6. 이미지 위에 얼굴 상자를 그리고 감정 설명을 붙이는 메서드 DrawFaceBox()
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

#TODO: 7. 얼굴의 대표 감정을 얻어와 얼굴 사각형 표시하는 메서드 DisplayEmotion()
private void DisplayEmotion(BitmapImage softwareBitmap, DetectedFace emotion)
{
    if (emotion != null)
    {
        var emotionName = EmotionHelper.GetTopEmotionName(emotion);

        DrawFaceBox(softwareBitmap, emotion.FaceRectangle, emotionName);
    }
}

#TODO: 8. 감정 인식 수행 버튼 이벤트 처리기를 완성합니다.
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