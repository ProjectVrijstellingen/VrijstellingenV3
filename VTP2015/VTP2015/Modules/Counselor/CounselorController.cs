﻿using System.Linq;
using System.Web.Mvc;
using AutoMapper.QueryableExtensions;
using Rotativa;
using VTP2015.Config;
using VTP2015.Modules.Counselor.DTOs;
using VTP2015.Modules.Counselor.ViewModels;
using VTP2015.ServiceLayer.Counselor;
using VTP2015.ServiceLayer.Counselor.Models;
using Evidence = VTP2015.Modules.Counselor.DTOs.Evidence;
using File = VTP2015.Modules.Counselor.DTOs.File;
using Module = VTP2015.Modules.Counselor.DTOs.Module;
using Partim = VTP2015.Modules.Counselor.DTOs.Partim;
using PrevEducation = VTP2015.Modules.Counselor.DTOs.PrevEducation;

namespace VTP2015.Modules.Counselor
{
    [Authorize(Roles = "Counselor")]
    [RoutePrefix("Counselor")]
    public class CounselorController : Controller
    {
        private readonly ICounselorFacade _counselorFacade;

        private readonly ConfigFile _configFile;

        public CounselorController(ICounselorFacade counselorFacade)
        {
            _counselorFacade = counselorFacade;
            _configFile = new ConfigFile();
        }


        //
        // GET: /Counselors/
        [Route("")]
        [HttpGet]
        public ViewResult Index()
        {
            return View();
        }

        [Route("GetFileDetailsById/{fileId}")]
        [HttpGet]
        public JsonResult GetFileDetailsById(int fileId)
        {
            if (!_counselorFacade.IsFileAvailable(fileId)) return Json(null);
            var file = _counselorFacade.GetFileByFileId(fileId);

            var dto = new File
            {
                StudentName = file.StudentFirstName + " " + file.StudentName,
                AmountOfUntreatedRequests = file.AmountOfUntreatedRequests,
                AmountOfApprovedRequests = file.AmountOfApprovedRequests,
                AmountOfDeniedRequests = file.AmountOfDeniedRequests,
                Modules = file.Modules.Select(m => new Module
                {
                    Name = m.Name,
                    Partims = m.Partims.Select(p => new Partim
                    {
                        FileId = p.FileId,
                        Name = p.Name,
                        RequestId = p.RequestId,
                        Status = p.Status.ToString(),
                        Evidence = p.Evidence.Select(e => new Evidence
                        {
                            Path = Server.MapPath("/bewijzen/" + file.StudentMail.Split('@')[0] + "/" + e.Path),
                            Argumentation = e.Description,
                            Type = e.Path.Split('.').Last()
                        }),
                        PrevEducations = p.PrevEducations.Select(e => new PrevEducation
                        {
                            Education = e.Education
                        }),
                        PartimInformationId = p.PartimInformationId
                    })
                })
            };

            return Json(dto, JsonRequestBehavior.AllowGet);
        }

        [Route("SetFileStatusOpen")]
        [HttpPost]
        public void SetFileStatusOpen(int fileId)
        {
            _counselorFacade.SetFileStatusOpen(fileId);
        }

        [Route("RemovePartimFromFile")]
        [HttpPost]
        public void RemovePartimFromFile(int partimInformationId, int fileId)
        {
            _counselorFacade.RemovePartimFromFile(partimInformationId, fileId);
        }

        [Route("DeleteFile")]
        [HttpPost]
        public void DeleteFile(int fileId)
        {
            _counselorFacade.DeleteFile(fileId);
        }

        [Route("EducationSelectWidget")]
        [HttpGet]
        public PartialViewResult EducationSelectWidget()
        {
            var viewModel = new EducationSelectViewModel
            {
                SelectedOpleiding = _counselorFacade.GetEducationNameByCounselorEmail(User.Identity.Name),
                Opleidingen = _counselorFacade.GetEducations()
                    .ProjectTo<EducationViewModel>()
            };

            return PartialView(viewModel);
        }

        [Route("ChangeEducation")]
        [HttpPost]
        public ActionResult ChangeOpleiding(string opleiding)
        {
            _counselorFacade.ChangeEducation(User.Identity.Name, opleiding);
            return Json("Changed!");
        }

        [Route("FileOverviewWidget")]
        [HttpGet]
        public PartialViewResult FileOverviewWidget()
        {
            var models = _counselorFacade.GetFilesByCounselorEmail(User.Identity.Name, _configFile.AcademieJaar())
                .ProjectTo<FileOverviewViewModel>();
            ViewBag.Lecturers = _counselorFacade.GetNrNoLecturersPartims(User.Identity.Name);

            return PartialView(models);

        }

        [Route("AssignLecturers")]
        [HttpGet]
        public ActionResult AssignLecturers()
        {
            //var models = _counselorFacade.GetPartimsNoLecturer(User.Identity.Name).ProjectTo<PartimInformationViewModel>();
            var models = _counselorFacade.GetAllPartims(User.Identity.Name).ProjectTo<PartimInformationViewModel>();
            return View(models.ToArray());
        }

        [Route("SendReminder")]
        [HttpGet]
        public ActionResult SendReminder(int aanvraagId)
        {
            _counselorFacade.SendReminder(aanvraagId);
            return Content("email sent");
            //string email = _aanvraagRepository.GetEmailByAanvraagId(aanvraagId);
            //TimeSpan passedTimeSinceLastEmail = DateTime.Now.Subtract(_docentRepository.GetByEmail(email).WarningMail);
            //if (_configFile.WarningMailTimeIsAllowed(passedTimeSinceLastEmail))
            //{
            //    string bodyText = "Geachte \r \r ";
            //    string begeleider = User.Identity.StudentName;
            //    int aantalAanvragenWachtend = _aanvraagRepository.GetOnbehandeldeAanvragen(email).Count();
            //    string dringendeAanvraagPartimNaam = _aanvraagRepository.GetAanvraagById(aanvraagId).PartimInformation.Partims.StudentName;
            //    string dringendeAanvraagAanvragerNaam = _aanvraagRepository.GetAanvraagById(aanvraagId).FileName.Student.Email;

            //    bodyText += begeleider + " Wenst u er van op de hoogte te brengen dat de aanvraag betreffende " +
            //                    dringendeAanvraagPartimNaam + " aangevraagd door " +
            //                    dringendeAanvraagAanvragerNaam + " op dringende keuring wacht! Verder wachten er nog " +
            //                    aantalAanvragenWachtend + " aanvragen op uw keuring. U kunt deze aanvragen bekijken en keuren op het vrijstellingen web platform."
            //                    + "\r \r (Deze mail werd verstuurd vanop het webplatform op vraag van de betreffende trajectbegeleider, antwoorden op dit emailadres worden niet gelezen.)";
            //    _mailHelper.sendEmail(begeleider, email, bodyText);
            //    _docentRepository.ChangeWarningTime(email, DateTime.Now);

            //    return Content(email + " ontvangt een email met een herinnering.");
            //}
            //return Content(email + " heeft reeds minder dan " + _configFile.GetConfig().WarningMailFrequency + " (Dagen/Uren/Minuten) geleden een herinnering ontvangen. " + 
            //    "De administrator van dit platform verhindert dat u momenteel een email kunt sturen om spam tegen te gaan");
        }

        [OverrideAuthorization]
        [Route("File/{fileId}")]
        [HttpGet]
        public ActionResult File(int fileId)
        {
            var model = _counselorFacade.GetFile(fileId);
            return View(model);
        }

        [Route("PrintFile/{id}")]
        public ActionResult PrintFile(int id)
        {
            return new ActionAsPdf("File",new {fileId = id}){FileName = "Dossier" + id + ".pdf"};
        }

        [Route("AssignLecturer")]
        public ActionResult AssignLecturer(AssignLectorViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return
                    Json(
                        (from modelstate in ModelState.Values from error in modelstate.Errors select error.ErrorMessage)
                            .ToArray());
            return Json(_counselorFacade.AssignLector(viewModel.Email, viewModel.SuperCode));
        }
    }
}