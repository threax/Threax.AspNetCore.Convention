﻿using NJsonSchema;
using NJsonSchema.CodeGeneration.TypeScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Threax.AspNetCore.Halcyon.ClientGen
{
    public class TypescriptClientWriter
    {
        private IClientGenerator clientGenerator;

        public TypescriptClientWriter(IClientGenerator clientGenerator)
        {
            this.clientGenerator = clientGenerator;
        }

        public String CreateClient()
        {
            Dictionary<String, JsonSchema4> interfacesToWrite = new Dictionary<string, JsonSchema4>();

            using (var writer = new StringWriter())
            {
                writer.WriteLine(@"import * as hal from 'hr.halcyon.EndpointClient';
import { Fetcher } from 'hr.fetcher';");

                WriteClient(interfacesToWrite, writer);

                foreach (var write in interfacesToWrite.Values)
                {
                    var generator = new TypeScriptGenerator(write);
                    generator.Settings.TypeStyle = TypeScriptTypeStyle.Interface;
                    generator.Settings.MarkOptionalProperties = true;
                    var classes = generator.GenerateType(write.Title);
                    writer.WriteLine(classes.Code);
                }

                return writer.ToString();
            }
        }

        private void WriteClient(Dictionary<string, JsonSchema4> interfacesToWrite, TextWriter writer)
        {
            foreach (var client in clientGenerator.GetEndpointDefinitions())
            {
                writer.WriteLine($@"
export class {client.Name}ResultView {{
    private client: hal.HalEndpointClient;

    constructor(client: hal.HalEndpointClient) {{
        this.client = client;
    }}

    public get data(): {client.Name} {{
        return this.client.GetData<{client.Name}>();
    }}
");

                foreach (var link in client.Links)
                {
                    var returnClassOpen = "";
                    var returnClassClose = "";
                    var linkReturnType = "";
                    var linkQueryArg = "";
                    var linkRequestArg = "";

                    //Extract any interfaces that need to be written
                    if (link.EndpointDoc.QuerySchema != null)
                    {
                        if (!interfacesToWrite.ContainsKey(link.EndpointDoc.QuerySchema.Title))
                        {
                            interfacesToWrite.Add(link.EndpointDoc.QuerySchema.Title, link.EndpointDoc.QuerySchema);
                        }
                        linkQueryArg = $"query: {link.EndpointDoc.QuerySchema.Title}";
                    }

                    if (link.EndpointDoc.RequestSchema != null)
                    {
                        if (!interfacesToWrite.ContainsKey(link.EndpointDoc.RequestSchema.Title))
                        {
                            interfacesToWrite.Add(link.EndpointDoc.RequestSchema.Title, link.EndpointDoc.RequestSchema);
                        }
                        linkRequestArg = $"data: {link.EndpointDoc.RequestSchema.Title}";
                    }

                    if (link.EndpointDoc.ResponseSchema != null)
                    {
                        if (!interfacesToWrite.ContainsKey(link.EndpointDoc.ResponseSchema.Title))
                        {
                            interfacesToWrite.Add(link.EndpointDoc.ResponseSchema.Title, link.EndpointDoc.ResponseSchema);
                        }
                        linkReturnType = $": Promise<{link.EndpointDoc.ResponseSchema.Title}ResultView>";
                        returnClassOpen = $"new {link.EndpointDoc.ResponseSchema.Title}ResultView(";
                        returnClassClose = ")";
                    }

                    var func = "LoadLink";
                    var inArgs = "";
                    var outArgs = "";
                    bool bothArgs = false;
                    if (linkQueryArg != "")
                    {
                        inArgs = linkQueryArg;
                        func = "LoadLinkWithQuery";
                        outArgs = ", query";

                        if (linkRequestArg != "")
                        {
                            inArgs += ", ";
                            func = "LoadLinkWithQueryAndBody";
                            bothArgs = true;
                        }
                    }

                    if (linkRequestArg != "")
                    {
                        inArgs += linkRequestArg;
                        outArgs += ", data";
                        if (!bothArgs)
                        {
                            func = "LoadLinkWithBody";
                        }
                    }

                    var funcName = link.Rel;
                    if (link.Rel == "self")
                    {
                        //Self links make refresh functions, also clear in and out args
                        funcName = "refresh";
                        inArgs = "";
                        outArgs = "";
                        func = "LoadLink";
                    }

                    //Write link
                    writer.WriteLine($@"
    public {funcName}({inArgs}){linkReturnType} {{
        return this.client.{func}(""{link.Rel}""{outArgs})
               .then(r => {{
                    return {returnClassOpen}r{returnClassClose};
                }});
    }}

    public can{funcName}(): boolean {{
        return this.client.HasLink(""{link.Rel}"");
    }}
");
                }

                //Close class
                writer.WriteLine(@"
}");
            }
        }
    }
}
