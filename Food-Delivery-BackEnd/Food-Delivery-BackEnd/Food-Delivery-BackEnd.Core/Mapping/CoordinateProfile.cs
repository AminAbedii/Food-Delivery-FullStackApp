using AutoMapper;
using Food_Delivery_BackEnd.Core.Dto.Shared;
using Food_Delivery_BackEnd.Core.Converters;
using NetTopologySuite.Geometries;

namespace Food_Delivery_BackEnd.Core.Mapping
{
    public class CoordinateProfile : Profile
    {
        public CoordinateProfile()
        {
            CreateMap<CoordinateDto, Coordinate>().ReverseMap();

            CreateMap<Coordinate, Point>().ConvertUsing(new CoordinateToPointConverter());

            CreateMap<List<Coordinate>, Polygon>().ConvertUsing(new CoordinatesToPolygonConverter());
        }
    }
}
