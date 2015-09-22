using System;

namespace SoundBoard.Updating.Internal
{
    internal class ProductUpdateFailedException : Exception
    {
        public const string ProductUpdateFailedMessage = "Product update failed exception. See the inner exception for more details.";

        public ProductUpdateFailedException(Exception innerException) : base(ProductUpdateFailedMessage,innerException)
        {
 
        }

    }
}
