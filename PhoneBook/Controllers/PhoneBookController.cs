using Microsoft.AspNetCore.Mvc;
using PhoneBook.Exceptions;
using PhoneBook.Model;
using PhoneBook.Services;
using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;

namespace PhoneBook.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PhoneBookController : ControllerBase
    {
        private readonly IPhoneBookService _phoneBookService;
        private readonly ILogger<PhoneBookController> _logger;

        public PhoneBookController(IPhoneBookService phoneBookService, ILogger<PhoneBookController> logger)
        {
            _phoneBookService = phoneBookService;
            _logger = logger;
        }

        [HttpGet]
        [Route("list")]
        public IEnumerable<PhoneBookEntry> List()
        {
            return _phoneBookService.List();
        }

        [HttpPost]
        [Route("add")]
        public IActionResult Add([FromBody] PhoneBookEntry newEntry)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _phoneBookService.Add(newEntry);

                return Ok();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }



        }

        [HttpPut]
        [Route("deleteByName")]
        public IActionResult DeleteByName([FromQuery] string name)
        {
            try
            {
                _phoneBookService.DeleteByName(name);

                return Ok();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut]
        [Route("deleteByNumber")]
        public IActionResult DeleteByNumber([FromQuery] string number)
        {
            try
            {
                _phoneBookService.DeleteByNumber(number);

                return Ok();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}