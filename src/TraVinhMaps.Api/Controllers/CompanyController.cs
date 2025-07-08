// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Features.Company.Interface;
using TraVinhMaps.Application.Features.Company.Mappers;
using TraVinhMaps.Application.Features.Company.Models;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;
    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }
    [HttpGet]
    [Route("GetAllCompany")]
    public async Task<IActionResult> GetAllCompany()
    {
        var listOcopType = await _companyService.ListAllAsync();

        return this.ApiOk(listOcopType);
    }
    [HttpGet]
    [Route("GetCompanyById/{id}", Name = "GetCompanyById")]
    public async Task<IActionResult> GetCompanyById(string id)
    {
        var ocopType = await _companyService.GetByIdAsync(id);
        return this.ApiOk(ocopType);
    }
    [HttpGet]
    [Route("CountCompanies")]
    public async Task<IActionResult> CountCompanies()
    {
        var countOcopTypes = await _companyService.CountAsync();
        return this.ApiOk(countOcopTypes);
    }
    [HttpPost]
    [Route("AddCompany")]
    public async Task<IActionResult> AddCompany([FromBody] CreateCompanyRequest createCompanyRequest)
    {
        var createCompany = CompanyMapper.Mapper.Map<Company>(createCompanyRequest);
        var company = await _companyService.AddAsync(createCompany);
        return CreatedAtRoute("GetCompanyById", new { id = company.Id }, this.ApiOk(company));
    }
    [HttpPut]
    [Route("UpdateCompany")]
    public async Task<IActionResult> UpdateCompany([FromBody] UpdateCompanyRequest updateCompanyRequest)
    {
        var existingCompany = await _companyService.GetByIdAsync(updateCompanyRequest.Id);
        if (existingCompany == null)
        {
            throw new NotFoundException("Company not found.");
        }

        existingCompany.Name = updateCompanyRequest.Name;
        existingCompany.Address = updateCompanyRequest.Address;
        existingCompany.Locations = updateCompanyRequest.Locations;
        existingCompany.Contact = updateCompanyRequest.Contact;
        if (updateCompanyRequest.UpdateAt.HasValue)
        {
            existingCompany.UpdateAt = updateCompanyRequest.UpdateAt.Value;
        }
        await _companyService.UpdateAsync(existingCompany);
        return this.ApiOk("Company updated successfully.");
    }
}
