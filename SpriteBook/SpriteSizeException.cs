using System;

namespace SpriteBook
{
    [Serializable]
    public class SpriteSizeException : Exception
    {
        public SpriteSizeException()
            : base()
        { }

        public SpriteSizeException(string message)
            : base(message)
        { }
    }
}