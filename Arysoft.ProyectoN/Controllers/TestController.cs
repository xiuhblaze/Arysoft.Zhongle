﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Arysoft.ProyectoN.Controllers
{
    public class TestController : ApiController
    {
        //public TestController() { } 

        // GET api/ 
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api//5 
        public string Get(int id)
        {
            return "value";
        }
    }
}

