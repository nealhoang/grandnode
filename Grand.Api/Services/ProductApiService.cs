﻿using Grand.Api.DTOs.Catalog;
using Grand.Api.Extensions;
using Grand.Data;
using Grand.Services.Catalog;
using Grand.Services.Seo;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;

namespace Grand.Api.Services
{
    public partial class ProductApiService : IProductApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;

        private readonly IMongoCollection<ProductDto> _product;

        public ProductApiService(IMongoDBContext mongoDBContext, IProductService productService, IUrlRecordService urlRecordService)
        {
            _mongoDBContext = mongoDBContext;
            _productService = productService;
            _urlRecordService = urlRecordService;

            _product = _mongoDBContext.Database().GetCollection<ProductDto>(typeof(Core.Domain.Catalog.Product).Name);
        }

        public virtual ProductDto GetById(string id)
        {
            return _product.AsQueryable().FirstOrDefault(x => x.Id == id);
        }
        public virtual IMongoQueryable<ProductDto> GetProducts()
        {
            return _product.AsQueryable();
        }

        public virtual ProductDto InsertOrUpdateProduct(ProductDto model)
        {
            if (string.IsNullOrEmpty(model.Id))
                model = InsertProduct(model);
            else
                model = UpdateProduct(model);

            return model;
        }

        public virtual ProductDto InsertProduct(ProductDto model)
        {
            var product = model.ToEntity();
            product.CreatedOnUtc = DateTime.UtcNow;
            product.UpdatedOnUtc = DateTime.UtcNow;
            _productService.InsertProduct(product);

            model.SeName = product.ValidateSeName(model.SeName, product.Name, true);
            product.SeName = model.SeName;
            //search engine name
            _urlRecordService.SaveSlug(product, model.SeName, "");
            _productService.UpdateProduct(product);
            return product.ToModel();
        }

        public virtual ProductDto UpdateProduct(ProductDto model)
        {
            //product
            var product = _productService.GetProductById(model.Id);
            product = model.ToEntity(product);
            product.UpdatedOnUtc = DateTime.UtcNow;
            model.SeName = product.ValidateSeName(model.SeName, product.Name, true);
            product.SeName = model.SeName;
            //search engine name
            _urlRecordService.SaveSlug(product, model.SeName, "");

            _productService.UpdateProduct(product);
            return product.ToModel();
        }
        public virtual void DeleteProduct(ProductDto model)
        {
            var product = _productService.GetProductById(model.Id);
            if (product != null)
                _productService.DeleteProduct(product);
        }

    }
}
