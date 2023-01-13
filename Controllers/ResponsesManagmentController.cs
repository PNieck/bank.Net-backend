﻿using System.Net.Mime;
using Bank.NET___backend.Data;
using Bank.NET___backend.Models;
using Bank.NET___backend.Models.QueryParametres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Bank.NET___backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponsesManagmentController : ControllerBase
    {
        private readonly SqlContext _sqlContext;
        public ResponsesManagmentController(SqlContext sqlContext)
        {
            _sqlContext = sqlContext;
        }

        //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
        [HttpGet("GetInquiries")]
        //[Authorize(Policy = "BankEmployee")]
        public ActionResult<IEnumerable<CompleteRequest>> GetInquiries([FromQuery] ResponseQueryParameters parametres)
        {
            if (!parametres.ValidateParameters())
            {
                return BadRequest();
            }

            var requests = _sqlContext.Requests.Where(req => req.ResponseID == null && req.External == false).ToList();
            List<CompleteRequest> data = new List<CompleteRequest>(requests.Count());

            foreach (Request request in requests)
            {
                data.Add(new CompleteRequest(request, null));
            }

            var result = parametres.handleQueryParametres(data.AsQueryable());
            HttpContext.Response.Headers.Add("PagingInfo", parametres.GetPagingMetadata(data.Count));

            return Ok(result);
        }

        //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
        [HttpGet("GetRequests")]
        //[Authorize(Policy = "BankEmployee")]
        public ActionResult<IEnumerable<CompleteRequest>> GetRequests([FromQuery] ResponseQueryParameters parametres)
        {
            if (!parametres.ValidateParameters())
            {
                return BadRequest();
            }

            var requests = _sqlContext.Requests.Where(req => req.ResponseID != null && req.External == false).ToList();
            List<CompleteRequest> data = new List<CompleteRequest>(requests.Count());

            foreach (Request request in requests)
            {
                Response? res = _sqlContext.Responses.Where(res => res.ResponseID == request.ResponseID).FirstOrDefault();

                if (res == null)
                    continue;

                data.Add(new CompleteRequest(res, request));
            }

            var result = parametres.handleQueryParametres(data.AsQueryable());
            HttpContext.Response.Headers.Add("PagingInfo", parametres.GetPagingMetadata(data.Count));

            return Ok(result);
        }

        [HttpGet]
        [Route("isUserABankEmployee")]
        [Authorize]
        public ActionResult<bool> isUserABankEmployee()
        {
            var claims = User.Claims.ToList();
            var EmailClaim = Helpers.GetClaim(claims, "emails");
            if (EmailClaim == null)
            {
                return NotFound();
            }

            if (_sqlContext.Admins.Where(ad => ad.Email == EmailClaim.Value).Any())
                return Ok(true);

            return Ok(false);
        }

        //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
        [HttpGet]
        [Route("/GetResponse/{ResponseId}")]
        //[Authorize(Policy = "BankEmployee")]
        public ActionResult<CompleteRequest> GetResponse(int ResponseId)
        {
            var request = _sqlContext.Requests.Where(req => req.ResponseID == ResponseId).FirstOrDefault();

            if (request == null)
                return BadRequest();

            var response = _sqlContext.Responses.Where(res => res.ResponseID == ResponseId).FirstOrDefault();

            if (response == null)
                return BadRequest();

            return Ok(new CompleteRequest(response, request));
        }

        [HttpGet("GetSortingParameters")]
        //[HttpPost("Approve")]
        //[Authorize(Policy = "BankEmployee")]
        public ActionResult<string[]> GetResponseSortingParameters()
        {
            return Ok(Enum.GetNames(typeof(ResponseSortingParameters)));
        }

        [HttpGet("GetResponsesStates")]
        //[HttpPost("Approve")]
        //[Authorize(Policy = "BankEmployee")]
        public ActionResult<string[]> GetResponsesStates()
        {
            return Ok(Enum.GetNames(typeof(ResponseStatus)));
        }

        //[RequiredScope("access_as_user")]
        [HttpPost]
        //[Authorize(Policy = "BankEmployee")]
        [Route("/ApproveResponse/{ResponseId}")]
        public ActionResult ApproveResponse(int ResponseId)
        {
            try
            {
                Response? res = _sqlContext.Responses.Where(res => res.ResponseID == ResponseId).FirstOrDefault();

                if (res == null)
                    return BadRequest();

                res.State = ResponseStatus.Approved.ToString();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        //[RequiredScope("access_as_user")]
        [HttpPost]
        //[Authorize(Policy = "BankEmployee")]
        [Route("/RefuseResponse/{ResponseId}")]
        public ActionResult RefuseResponse(int ResponseId)
        {
            try
            {
                Response? res = _sqlContext.Responses.Where(res => res.ResponseID == ResponseId).FirstOrDefault();

                if (res == null)
                    return BadRequest();

                res.State = ResponseStatus.Refused.ToString();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost("GetAgreement/{rqid}")]
        [Authorize(Policy = "BankEmployee")]
        [RequiredScope("access_as_user")]
        public ActionResult GetAgreement(int rqid)
        {
            try
            {
                Request req = _sqlContext.Requests.Where(r => r.RequestID == rqid).First();
                ;
                var stream = Helpers.downloadDocument("dotnet-bank-agreements",req.AgreementKey);
                stream.Seek(0, SeekOrigin.Begin);
                //return File(stream, "agreement.jpg");
                //MediaTypeNames.Image.Jpeg.ToString();
                return File(stream, MediaTypeNames.Text.Plain,"agreement.txt");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        [HttpPost("GetDocument/{rqid}")]
        [Authorize(Policy = "BankEmployee")]
        [RequiredScope("access_as_user")]
        public ActionResult GetDocument(int rqid)
        {
            try
            {
                Request req = _sqlContext.Requests.Where(r => r.RequestID == rqid).First();
                var stream = Helpers.downloadDocument("dotnet-bank-documents",req.DocumentKey);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, MediaTypeNames.Image.Jpeg, "document.jpg");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
