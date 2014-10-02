using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using umbraco.cms.businesslogic.web;

namespace mastrosoft.Umbraco.LanguageSelector
{
    public class LanguageHttpModule : IHttpModule
    {
        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
            
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

        /// <summary>
        /// Handles the BeginRequest event of the context control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            List<string> languageList = new List<string>();
            foreach (string s in request.UserLanguages)
            {
                languageList.Add(s.Split(new char[] { ';' })[0]);
            }
            if (!string.IsNullOrEmpty(LanguageHttpHandler.Language))
            {
                languageList.Insert(0, LanguageHttpHandler.Language);
            }

            string[] languages = languageList.ToArray();
            if(request.Url.AbsolutePath == "/" || request.Url.AbsolutePath.Equals("/default.aspx",StringComparison.InvariantCultureIgnoreCase)){
                if (languages.Length > 0)
                {
                    //context.Response.Write(string.Join("(--)", languages));
                    //context.Response.End();
                    // Iterate all languages for the user...
                    foreach (string lang in languages)
                    {
                        context.Response.Write(lang + "-");
                        Document[] rootNodes = Document.GetRootDocuments();
                        foreach (Document node in rootNodes)
                        {
                            // iterate trough all (published) root nodes and check their domains...
                            if (node.Published)
                            {

                                Domain[] domainList = Domain.GetDomainsById(node.Id);
                                foreach (Domain domain in domainList)
                                {
                                    // If the domain has the same language as the user, redirect to it...
                                    if (domain.Language.CultureAlias.StartsWith(lang, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        /*if (!string.IsNullOrEmpty(domain.Name))
                                        {
                                            response.Redirect(umbraco.library.NiceUrlWithDomain(node.Id));
                                        }
                                        else
                                        {*/
                                            response.Redirect(umbraco.library.NiceUrl(node.Id));
                                        //}
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
