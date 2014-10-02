using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.relation;
using System.Globalization;

namespace mastrosoft.Umbraco.LanguageSelector
{
    public class LanguageHttpHandler : IHttpHandler
    {
        private const string QS_ID = "ID";
        private const string QS_CULTURE = "Culture";
        private const string COOKIE_NAME = "mastrosoft.UmbracoLanguage";
        public static string Language
        {
            set
            {
                /*if (HttpContext.Current.Response.Cookies[COOKIE_NAME] == null)
                {*/
                    HttpCookie cookie = new HttpCookie(COOKIE_NAME, value);
                    cookie.Expires = DateTime.Now.AddYears(1);
                    HttpContext.Current.Response.Cookies.Add(cookie);
                /*}
                else
                {
                    HttpContext.Current.Response.Cookies[COOKIE_NAME].Value = value;
                    HttpContext.Current.Response.Cookies[COOKIE_NAME].Expires = DateTime.Now.AddYears(1);
                }*/
            }
            get
            {
                if (HttpContext.Current.Request.Cookies[COOKIE_NAME] != null)
                {
                    return HttpContext.Current.Request.Cookies[COOKIE_NAME].Value;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the root document for a document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        private Document GetRoot(Document document)
        {
            while (document.ParentId > 0)
            {
                document = new Document(document.ParentId);
            }
            return document;
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            string culture = context.Request.QueryString[QS_CULTURE];
            int nodeID = 0;
            if (int.TryParse(context.Request.QueryString[QS_ID],out nodeID))
            {
                // Get domains connected to current node
                Domain[] domains = Domain.GetDomainsById(nodeID);

                // Load current node
                Document d = new Document(nodeID);
                Language = culture;
                // Get its relations
                Relation[] relations = d.Relations;
                if (relations.Length == 0)
                {
                    foreach (Document doc in Document.GetRootDocuments())
                    {
                        if (doc.Published)
                        {
                            /*if (doc.Id != nodeID)
                            {*/
                            Domain[] domainsForRootNode = Domain.GetDomainsById(doc.Id);
                            if (domainsForRootNode.Any(n => n.Language.CultureAlias.StartsWith(culture, StringComparison.InvariantCultureIgnoreCase)))
                            {
                               // Language = culture;
                                context.Response.Redirect(umbraco.library.NiceUrl(doc.Id));
                                return;
                            }
                            //}
                        }
                    }
                }
                else
                {
                    foreach (Relation r in relations)
                    {
                        if (r.RelType.Alias.Equals("relateDocumentOnCopy", StringComparison.InvariantCultureIgnoreCase))
                        {

                            Document relatedDocument = new Document(nodeID==r.Parent.Id?r.Child.Id:r.Parent.Id);
                            Document rootDocument = GetRoot(relatedDocument);
                            Document originalRootDocument = GetRoot(d);

                            /*context.Response.Write(umbraco.library.NiceUrl(relatedDocument.Id) + "<br/>");
                            context.Response.Write(umbraco.library.NiceUrl(rootDocument.Id) + "<br/>");
                            context.Response.Write(umbraco.library.NiceUrl(originalRootDocument.Id) + "<br/>");*/

                            /*if (nodeID != originalRootDocument.Id)
                            {*/
                                Domain[] relatedDomains = Domain.GetDomainsById(rootDocument.Id);
                                foreach (Domain rd in relatedDomains)
                                {
                                    // if language is not added to the current node aswell continue...
                                    if (!domains.Any(n => n.Language.CultureAlias.Equals(rd.Language.CultureAlias, StringComparison.InvariantCultureIgnoreCase)))
                                    {
                                        string tempCulture = culture;
                                        if (tempCulture.Length < 5)
                                        {
                                            tempCulture = CultureInfo.CreateSpecificCulture(culture).ToString();
                                        }

                                        if (rd.Language.CultureAlias.StartsWith(culture, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            
                                            context.Response.Redirect(umbraco.library.NiceUrl(relatedDocument.Id));
                                            return;
                                        }/*else

                                    // if language is the selected language, continue
                                    if (rd.Language.CultureAlias.Equals(culture, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        context.Response.Redirect(umbraco.library.NiceUrl(relatedDocument.Id));
                                        return;
                                    }*/
                                    }
                                }
                           // }
                            /*Domain[] relatedDomains = Domain.GetDomainsById(rootDocument.Id);
                            foreach (Domain rd in relatedDomains)
                            {
                                // if language is not added to the current node
                                if (!domains.Any(n => n.Language.CultureAlias.Equals(rd.Language.CultureAlias, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    context.Response.Redirect(umbraco.library.NiceUrl(relatedDocument.Id));
                                    return;
                                }
                            }*/
                        }
                    }
                    context.Response.Redirect(umbraco.library.NiceUrl(nodeID));
                }
            }
        }
    }
}
