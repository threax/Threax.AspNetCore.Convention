﻿using Halcyon.HAL;
using Halcyon.HAL.Attributes;
using Halcyon.Web.HAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Threax.AspNetCore.Halcyon.Ext
{
    public class HalModelResultFilterAttribute : ResultFilterAttribute
    {
        private IHALConverter halConverter;

        public HalModelResultFilterAttribute(IHALConverter halConverter)
        {
            this.halConverter = halConverter;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            //Controller must extend ControllerBase
            if (!(context.Controller is ControllerBase))
            {
                throw new InvalidOperationException($"Controller {context.Controller.GetType().FullName} does not extend ControllerBase or Controller. It must do this to work with hal.");
            }

            var objResult = context.Result as ObjectResult;
            if (objResult != null)
            {
                HALResponse halResponse = objResult.Value as HALResponse;
                if(halResponse == null && !(objResult.Value is String))
                {
                    halResponse = halConverter.Convert(objResult.Value);
                }
                if (halResponse != null)
                {
                    context.Result = halResponse.ToActionResult(context.Controller as ControllerBase);
                }
            }
        }
    }
}