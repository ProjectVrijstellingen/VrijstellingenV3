﻿using System.Linq;
using System.Web.Mvc;
using VTP2015.lib;
using VTP2015.Modules.Student.ViewModels;
using VTP2015.ServiceLayer.Student.Models;

namespace VTP2015.Helpers
{
    public static class CustomHtmlHelpers
    {
        public static MvcHtmlString ShowPartimList(this HtmlHelper html, PartimViewModel[] viewModels, bool deletable)
        {
            var htmlString = "";
            var partimFactory = new PartimFactory(viewModels);
            var status = new string[] { "", " - onbehandeld", " - afgekeurd", " - goedgekeurd"};
            foreach (var semester in partimFactory.Semesters.OrderBy(x => x.Number))
            {
                var semesterTag = new TagBuilder("div");
                semesterTag.AddCssClass("semesterDiv");
                semesterTag.Attributes.Add("data-semester", semester.Number.ToString());

                var semesterNameTag = new TagBuilder("div");
                semesterNameTag.InnerHtml += ShowGlyphicon(html, "triangle-right");
                semesterNameTag.AddCssClass("semester");

                var nameTag = new TagBuilder("span");
                nameTag.AddCssClass("h3");
                nameTag.SetInnerText(" Semester " + semester.Number);
                semesterNameTag.InnerHtml += nameTag;
                semesterTag.InnerHtml += semesterNameTag;

                var moduleListTag = new TagBuilder("div");
                moduleListTag.AddCssClass("hide partimList");


                foreach (var module in semester.Modules)
                {
                    var count = module.Partims.Count;
                    var submitted = module.Partims.All(x => x.Status != 0);
                    var tag = new TagBuilder("div");
                    tag.Attributes.Add("data-moduleid", module.Code);
                    tag.AddCssClass("moduleSpace");

                    var moduleNameTag = new TagBuilder("span");
                    moduleNameTag.AddCssClass("name h4" + (count == module.RequestCount && count == module.TotalCount && module.TotalCount != 1 && !submitted ? " module" : ""));
                    moduleNameTag.SetInnerText(module.Name);
                    tag.InnerHtml += moduleNameTag;
                    if (!submitted)
                        tag.InnerHtml += ShowGlyphicon(html, "remove",
                            "btn crossModule" + (count == module.RequestCount && count == module.TotalCount && module.TotalCount != 1 && deletable ? "" : " hide"));

                    var moduleTag = new TagBuilder("ul");
                    moduleTag.AddCssClass("list-group" +
                                          (count == module.RequestCount && count == module.TotalCount && module.TotalCount != 1 && deletable && !submitted ? " hide" : ""));
                    foreach (var partim in module.Partims)
                    {
                        var partimTag = new TagBuilder("li");
                        partimTag.Attributes.Add("data-SuperCode", partim.SuperCode);
                        partimTag.AddCssClass("list-group-item" + (partim.Status == 0 ? " partim" : " status"));
                        var partimNameTag = new TagBuilder("span");
                        if (TextLimiter(partim.Name, 30).EndsWith("..."))
                        {
                            partimNameTag.MergeAttribute("data-toggle", "tooltip");
                            partimNameTag.MergeAttribute("title", partim.Name);

                        }
                        partimNameTag.AddCssClass("name");
                        //partimNameTag.SetInnerText(TextLimiter(partim.Name, 100) + status[partim.Status]);
                        partimNameTag.SetInnerText(TextLimiter(partim.Name, 100));
                        partimTag.InnerHtml += partimNameTag;
                        if (partim.Status == 0)
                            partimTag.InnerHtml += ShowGlyphicon(html, "remove", "btn crossPartim" + (deletable ? "" : " hide"));
                        else if (partim.Status == 2)
                            partimTag.InnerHtml += ShowGlyphicon(html, "ban-circle", "ban");
                        else if (partim.Status == 3)
                            partimTag.InnerHtml += ShowGlyphicon(html, "ok", "vinkje");
                        moduleTag.InnerHtml += partimTag;
                    }
                    tag.InnerHtml += moduleTag;
                    moduleListTag.InnerHtml += tag;
                }
                semesterTag.InnerHtml += moduleListTag;
                htmlString += semesterTag.ToString();
            }
            return new MvcHtmlString(htmlString);
        }

        public static MvcHtmlString ShowBewijzenList(this HtmlHelper html, EvidenceListViewModel[] bewijzen, bool movable)
        {
            return ShowBewijzenList(html, bewijzen, movable, false);
        }

        public static MvcHtmlString ShowBewijzenList(this HtmlHelper html, EvidenceListViewModel[] bewijzen, bool movable, bool submitted)
        {
            var tag = new TagBuilder("ul");
            tag.AddCssClass("list-group");
            tag.Attributes.Add("id", movable ? "dragBewijzenList" : "bewijzenList");
            foreach (var bewijs in bewijzen)
            {
                tag.InnerHtml += ShowBewijsLi(html, bewijs, movable, submitted);
            }
            return new MvcHtmlString(tag.ToString());
        }

        public static MvcHtmlString ShowBewijsLi(this HtmlHelper html, EvidenceListViewModel evidence, bool movable, bool submitted)
        {
            var itemTag = new TagBuilder("li");
            itemTag.AddCssClass("list-group-item");
            itemTag.Attributes.Add("data-bewijsid", evidence.Id.ToString());
            if (movable) itemTag.Attributes.Add("id", "evidence-" + evidence.Id);
            itemTag.InnerHtml += ShowGlyphicon(html, "file");
            var descriptionTag = new TagBuilder("span");
            descriptionTag.AddCssClass("glyphicon-class");
            if (TextLimiter(evidence.Path, 20).EndsWith("..."))
            {
                descriptionTag.MergeAttribute("data-toggle", "tooltip");
                descriptionTag.MergeAttribute("title", evidence.Path);
            }
            descriptionTag.SetInnerText(" " + TextLimiter(evidence.Path,20) + " - " + evidence.Description);
            itemTag.InnerHtml += descriptionTag;
            if (!submitted) itemTag.InnerHtml += ShowGlyphicon(html, "remove","btn crossPartim" + (movable? " hide":""));
            if (movable) itemTag.InnerHtml += ShowGlyphicon(html, "plus","btn crossPartim");
            return new MvcHtmlString(itemTag.ToString());
        }

        public static MvcHtmlString ShowEducationList(this HtmlHelper html, EducationListViewModel[] educationList, bool movable)
        {
            return ShowEducationList(html, educationList, movable, false);
        }

        public static MvcHtmlString ShowEducationList(this HtmlHelper html, EducationListViewModel[] educationList, bool movable, bool submitted)
        {
            var tag = new TagBuilder("ul");
            tag.AddCssClass("list-group");
            tag.Attributes.Add("id", movable ? "dragEducationList" : "educationList");
            foreach (var bewijs in educationList)
            {
                tag.InnerHtml += ShowEducationLi(html, bewijs, movable, submitted);
            }
            return new MvcHtmlString(tag.ToString());
        }

        public static MvcHtmlString ShowEducationLi(this HtmlHelper html, EducationListViewModel education, bool movable, bool submitted)
        {
            var itemTag = new TagBuilder("li");
            itemTag.AddCssClass("list-group-item");
            itemTag.Attributes.Add("data-educationid", education.Id.ToString());
            if (movable) itemTag.Attributes.Add("id", "education-" + education.Id);
            var descriptionTag = new TagBuilder("span");
            descriptionTag.AddCssClass("glyphicon-class");
            if (TextLimiter(education.Education, 100).EndsWith("..."))
            {
                descriptionTag.MergeAttribute("data-toggle", "tooltip");
                descriptionTag.MergeAttribute("title", education.Education);
            }
            descriptionTag.SetInnerText(" " + TextLimiter(education.Education, 100));
            itemTag.InnerHtml += descriptionTag;
            if (!submitted) itemTag.InnerHtml += ShowGlyphicon(html, "remove", "btn crossPartim" + (movable ? " hide" : ""));
            if (movable) itemTag.InnerHtml += ShowGlyphicon(html, "plus", "btn crossPartim");
            return new MvcHtmlString(itemTag.ToString());
        }

        public static MvcHtmlString ShowAanvraagDetails(this HtmlHelper html, RequestDetailViewModel[] aanvragen)
        {
            var htmlString = "";
            foreach (var aanvraag in aanvragen)
            {
                var articleTag = new TagBuilder("article");
                articleTag.Attributes.Add("id",aanvraag.Id.ToString());
                articleTag.Attributes.Add("data-code",aanvraag.Code);
                articleTag.Attributes.Add("data-requestId",aanvraag.Id.ToString());
                articleTag.AddCssClass("hide");
                var moduleTag = new TagBuilder("h3");
                moduleTag.SetInnerText(aanvraag.ModuleName);
                articleTag.InnerHtml += moduleTag;
                var partimTag = new TagBuilder("h4");
                partimTag.SetInnerText(aanvraag.PartimName);
                articleTag.InnerHtml += partimTag;
                if (aanvraag.Submitted && aanvraag.Motivation != "geen")
                {
                    var motivatieTag = new TagBuilder("p");
                    var headTag = new TagBuilder("b");
                    headTag.SetInnerText("Motivatie voor status: ");
                    motivatieTag.InnerHtml += headTag + "<br/>" + aanvraag.Motivation;
                    articleTag.InnerHtml += motivatieTag;
                }
                var educationLabelTag = new TagBuilder("label");
                educationLabelTag.AddCssClass("control-label");
                educationLabelTag.SetInnerText("Vorige opleidingen:");
                articleTag.InnerHtml += educationLabelTag;
                articleTag.InnerHtml += "<br />";

                var educationTag = new TagBuilder("ul");
                if (!aanvraag.Submitted) educationTag.Attributes.Add("id", "opleidingen");
                educationTag.AddCssClass("list-group");
                foreach (var opleiding in aanvraag.Educations)
                {
                    educationTag.InnerHtml += ShowEducationLi(html, opleiding, false, aanvraag.Submitted);
                }
                articleTag.InnerHtml += educationTag;
                articleTag.InnerHtml += "<br />";

                var bewijzenLabelTag = new TagBuilder("label");
                bewijzenLabelTag.AddCssClass("control-label");
                bewijzenLabelTag.SetInnerText("Bewijzen:");
                articleTag.InnerHtml += bewijzenLabelTag;

                var bewijzenTag = new TagBuilder("ul");
                if(!aanvraag.Submitted) bewijzenTag.Attributes.Add("id","bewijzen");
                bewijzenTag.AddCssClass("list-group");
                foreach (var bewijs in aanvraag.Evidence)
                {
                    bewijzenTag.InnerHtml += ShowBewijsLi(html, bewijs, false, aanvraag.Submitted);
                }
                articleTag.InnerHtml += bewijzenTag;
                articleTag.InnerHtml += "<br />";

                var buttonTag = new TagBuilder("button");
                buttonTag.AddCssClass("btn btn-primary");
                buttonTag.Attributes.Add("onclick", "Return()");
                buttonTag.Attributes.Add("type","button");
                buttonTag.SetInnerText("Terug");
                articleTag.InnerHtml += buttonTag;
                htmlString += articleTag.ToString();
            }
            return new MvcHtmlString(htmlString);
        }

        public static MvcHtmlString ShowGlyphiconWithRole(this HtmlHelper html, string name, string role)
        {
            var tag = new TagBuilder("span");
            tag.AddCssClass("glyphicon glyphicon-" + name);
            tag.Attributes.Add("aria-hidden", "true");
            tag.Attributes.Add("data-role", role);
            return new MvcHtmlString(tag.ToString());
        }

        public static MvcHtmlString ShowGlyphicon(this HtmlHelper html, string name, string classes)
        {
            var tag = new TagBuilder("span");
            tag.AddCssClass("glyphicon glyphicon-" + name + " " + classes);
            tag.SetInnerText(" ");
            return new MvcHtmlString(tag.ToString());
        }

        public static MvcHtmlString ShowGlyphicon(this HtmlHelper html, string name)
        {
            var tag = new TagBuilder("span");
            tag.AddCssClass("glyphicon glyphicon-" + name);
            tag.Attributes.Add("aria-hidden", "true");
            return new MvcHtmlString(tag.ToString());
        }

        public static string TextLimiter(string text, int length)
        {
            if (text.Length <= length) return text;
            return text.Substring(0, length - 1) + "...";
        }

        public static MvcHtmlString NextButton(this HtmlHelper html, string id)
        {
            TagBuilder htmlButton = new TagBuilder("button");

            htmlButton.Attributes.Add("id", id);
            htmlButton.Attributes.Add("class", "btn btn-primary");
            htmlButton.Attributes.Add("style", "float:right");
            htmlButton.InnerHtml += "volgende stap";

            return new MvcHtmlString(htmlButton.ToString());
        }
    }
}