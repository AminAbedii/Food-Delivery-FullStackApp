﻿namespace Food_Delivery_BackEnd.Core.Exceptions
{
    public class IncompatibleItemsError : Exception
    {
        public IncompatibleItemsError()
        {
        }

        public IncompatibleItemsError(string? message) : base(message)
        {
        }

        public IncompatibleItemsError(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
