using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard.AutoUpdate
{
    internal class ProductUpdateFailedException : Exception
    {
        public const string ProductUpdateFailedMessage = "Product update failed exception. See the inner exception for more details.";

        public ProductUpdateFailedException(Exception innerException) : base(ProductUpdateFailedMessage,innerException)
        {
 
        }

    }
}
