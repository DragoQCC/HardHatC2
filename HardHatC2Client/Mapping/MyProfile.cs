using ApiModels.Responses;
using AutoMapper;
using HardHatC2Client.Models;

namespace HardHatC2Client.Mapping
{
    public class MyProfile : Profile
    {
        public MyProfile()
        {
            CreateMap<ManagerResponse, Manager>();
            CreateMap<EngineerResponse, Engineer>();
            CreateMap<EngineerTaskResponse, EngineerTask>();
            CreateMap<Root, Engineer>();
        }
    }
}
