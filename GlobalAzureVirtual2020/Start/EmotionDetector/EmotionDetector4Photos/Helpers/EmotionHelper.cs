using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace EmotionsDetector.Helpers
{
    public static class EmotionHelper
    {
        // TODO: 01. 대표 감정을 가져오는 메서드를 작성합니다.  
        public static string GetTopEmotionName(DetectedFace emotion)
        {
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
        }

        // TODO: 02. 감정을 색과 연결하는 메서드를 만듭니다.
        public static Color GetEmotionColor(string emotionName)
        {
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
        }

        //TODO: 03. 얼굴 사각형위에 노출된 대표적인 감정의 텍스트 블록 배치를 준비하는 메서드를 만듭니다.
        public static TextBlock PrepareEmotionTextBlock(Rectangle faceBox, Color emotionColor, string emotionName)
        {
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
        }

        // TODO: 04. 감성 색을 반영한 얼굴 사각형을 준비하는 메서드를 만듭니다
        public static Rectangle PrepareFaceBox(FaceRectangle faceRectangle, Color emotionColor, double xScalingFactor, double yScalingFactor)
        {
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
        }
    }
}
