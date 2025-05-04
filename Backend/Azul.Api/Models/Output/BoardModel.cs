using AutoMapper;
using Azul.Core.BoardAggregate;
using Azul.Core.BoardAggregate.Contracts;

namespace Azul.Api.Models.Output;

public class BoardModel
{
    public PatternLineModel[] PatternLines { get; set; }
    public TileSpotModel[,] Wall { get; set; }
    public TileSpotModel[] FloorLine { get; set; }
    public int Score { get; set; }

    private class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<IBoard, BoardModel>();
        }
    }
}