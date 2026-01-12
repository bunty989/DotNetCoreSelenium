namespace NSWEHealth.Amazon
{
    internal class AmazonTestConstant
    {
        public enum BrandName
        {
            Sony,
            Philips,
            Samsung,
            Amazon,
            Generic,
            Tavice
        }

        public class DisplayTech
        {
            public const string OLED = "OLED";
            public const string LED = "LED";
            public const string LCD = "LCD";
            public const string QLED = "QLED";
        }

        public class ScreenSize
        {
            public const string SixtyToSixtyNine = "60-69 in";
            public const string ThirtyThreeToFortyThree = "33-43 in";
            public const string FiftyToFiftyNine = "50-59 in";
            public const string Seventy = "70 in";
        }

        public class DisplayResolution
        {
            public const string FourK = "4K/Ultra HD";
            public const string FullHD = "1080p/Full HD";
        }
    }
}