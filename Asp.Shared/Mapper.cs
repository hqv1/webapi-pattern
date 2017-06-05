using AutoMapper;
using Hqv.CSharp.Common.Web.Api;
using WebApiPattern.Asp.Shared.Models;
using WebApiPattern.Domain.Entities;
using IMapper = AutoMapper.IMapper;

namespace WebApiPattern.Asp.Shared
{
    /// <summary>
    /// Map entities to models
    /// Map models to entities
    /// </summary>
    public class Mapper : Hqv.CSharp.Common.Interfaces.IMapper
    {
        private readonly MapperConfiguration _mapperConfiguration;
        private readonly IMapper _mapper;

        public Mapper()
        {
            _mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductEntity, ProductForGetModel>();
                cfg.CreateMap<ProductEntity, ProductForUpdateModel>();
                cfg.CreateMap<ProductForCreationModel, ProductEntity>();
                cfg.CreateMap<ProductForUpdateModel, ProductEntity>();

                cfg.CreateMap<SellerEntity, SellerForGetModel>();
                cfg.CreateMap<SellerEntity, SellerForUpdateModel>();
                cfg.CreateMap<SellerForCreationModel, SellerEntity>();
                cfg.CreateMap<SellerForUpdateModel, SellerEntity>();

                cfg.CreateMap<Data.Sqlite.Models.ProductModel, ProductEntity>();
                cfg.CreateMap<PagedList<Data.Sqlite.Models.ProductModel>, PagedList<ProductEntity>>();
                cfg.CreateMap<Data.Sqlite.Models.SellerModel, SellerEntity>();
            });
            _mapper = _mapperConfiguration.CreateMapper();            
        }

        public TT Map<TT>(object source)
        {
            return _mapper.Map<TT>(source);
        }

        public void Map<TU, TT>(TU source, TT destination)
        {
            _mapper.Map(source, destination);
        }

        public void AssertConfigurationIsValid()
        {
            _mapperConfiguration.AssertConfigurationIsValid();
        }
    }
}