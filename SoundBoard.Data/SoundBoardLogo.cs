namespace SoundBoard.Data
{
    public class SoundBoardLogo
    {
        public string ImageSource { get; set; }

        public static SoundBoardLogo Default
        {
            get
            {
                return new SoundBoardLogo
                {
                    ImageSource = "Default.png"
                };
            }
        }
    }
}
