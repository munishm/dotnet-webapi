﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using CseLabs.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace CSApp.Controllers
{
    /// <summary>
    /// Handle benchmark requests
    /// </summary>
    [Route("api/[controller]")]
    public class BenchmarkController : Controller
    {
        private static readonly CseLog Logger = new ()
        {
            Name = typeof(BenchmarkController).FullName,
            ErrorMessage = "BenchmarkControllerException",
        };

        private static string benchmarkData;

        /// <summary>
        /// Returns a string value of benchmark data
        /// </summary>
        /// <param name="size">size of return</param>
        /// <response code="200">text/plain of size</response>
        /// <returns>IActionResult</returns>
        [HttpGet("{size}")]
        public async Task<IActionResult> GetDataAsync([FromRoute] int size)
        {
            IActionResult res;

            // validate size
            if (size < 1)
            {
                List<CseLabs.Middleware.Validation.ValidationError> list = new ()
                {
                    new CseLabs.Middleware.Validation.ValidationError
                    {
                        Target = "size",
                        Message = "size must be > 0",
                    },
                };

                Logger.LogWarning(nameof(GetDataAsync), "Invalid Size", CseLog.LogEvent400, HttpContext);

                return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
            }

            if (size > 1024 * 1024)
            {
                List<CseLabs.Middleware.Validation.ValidationError> list = new ()
                {
                    new CseLabs.Middleware.Validation.ValidationError
                    {
                        Target = "size",
                        Message = $"size must be <= 1 MB ({1024 * 1024})",
                    },
                };

                Logger.LogWarning(nameof(GetDataAsync), "Invalid Size", CseLog.LogEvent400, HttpContext);

                return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
            }

            // return exact byte size
            res = await ResultHandler.Handle(GetBenchmarkDataAsync(size), Logger).ConfigureAwait(false);

            return res;
        }

        private async Task<string> GetBenchmarkDataAsync(int size)
        {
            if (string.IsNullOrEmpty(benchmarkData))
            {
                benchmarkData = "0123456789ABCDEF";

                // 1 MB
                while (benchmarkData.Length < 1024 * 1024)
                {
                    benchmarkData += benchmarkData;
                }
            }

            return await Task<string>.Factory.StartNew(() =>
            {
                return benchmarkData[0..size];
            }).ConfigureAwait(false);
        }
    }
}
