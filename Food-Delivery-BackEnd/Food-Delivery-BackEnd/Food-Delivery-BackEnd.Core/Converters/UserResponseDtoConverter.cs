﻿//using Food_Delivery_BackEnd.Core.Dto;
//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace Food_Delivery_BackEnd.Core.Converters
//{
//    public class UserResponseDtoConverter : JsonConverter<UserResponseDto>
//    {
//        public override UserResponseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Write(Utf8JsonWriter writer, UserResponseDto value, JsonSerializerOptions options)
//        {
//            if (value is CustomerResponseDto customerDto)
//            {
//                JsonSerializer.Serialize(writer, customerDto, options);
//            }
//            else if (value is PartnerResponseDto partnerDto)
//            {
//                JsonSerializer.Serialize(writer, partnerDto, options);
//            }
//            else if (value is AdminResponseDto adminDto)
//            {
//                JsonSerializer.Serialize(writer, adminDto, options);
//            }
//            else
//            {
//                throw new NotSupportedException("Unsupported type: " + value.GetType().Name);
//            }
//        }
//    }
//}
