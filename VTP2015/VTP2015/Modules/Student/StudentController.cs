﻿using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using AutoMapper.QueryableExtensions;
using Microsoft.Web.Mvc;
using VTP2015.Config;
using VTP2015.Modules.Student.ViewModels;
using VTP2015.Security;
using VTP2015.ServiceLayer.Student;
using VTP2015.ServiceLayer.Student.Models;

namespace VTP2015.Modules.Student
{
    [Authorize(Roles = "Student")]
    [RoutePrefix("Student")]
    public class StudentController : Controller
    {
        private readonly IStudentFacade _studentFacade;

        public StudentController(IStudentFacade studentFacade)
        {
            _studentFacade = studentFacade;
        }

        #region index
        [Route("")]
        [HttpGet]
        public ViewResult Index()
        {
            return View();
        }

        [Route("")]
        [HttpPost]
        public ActionResult AddFile(IndexViewModel viewModel) /*het volledige dossier wegschrijven naar de database*/
        {

            ModelState.Clear();     
            var validation = TryValidateModel(viewModel);
            var errors = (from modelstate in ModelState.Values from error in modelstate.Errors select error.ErrorMessage).ToList();

            if (!validation) return Json(errors.ToArray());

            var pic = Path.GetFileName(viewModel.File.FileName);
            var name = User.Identity.Name.Split('@')[0];
            var path = Path.Combine(Server.MapPath("/bewijzen/" + name), pic);

            if (System.IO.File.Exists(path))
            {
                errors.Add("FileName does already exist!");
                return Json(errors.ToArray());
            }
            Directory.CreateDirectory(Server.MapPath("/bewijzen/"+name));
            viewModel.File.SaveAs(path);

            var dbBewijs = new Evidence
            {
                Path = pic,
                Description = viewModel.Description
            };

            _studentFacade.InsertEvidence(dbBewijs, User.Identity.Name);
            errors.Add("Finish");
            return Json(errors.ToArray());
        }

        [Route("DeleteEvidence")]
        [HttpPost]
        public ActionResult DeleteEvidence(int bewijsId) /* verwijder bewijs */
        {
            if (!_studentFacade.IsEvidenceFromStudent(User.Identity.Name))
                return Content("bewijs bestaat niet voor gebruiker!");

            var mapPath = Request.MapPath("/bewijzen/" + User.Identity.Name.Split('@')[0]);

            return Content(!_studentFacade.DeleteEvidence(bewijsId, mapPath)
                ? "gegeven bestand kon niet verwijdert worden!"
                : "Voltooid!");
        }

        [Route("InfoWidget")]
        [HttpGet]
        public ActionResult InfoWidget() /* toont de accountgegevens van de aangemelde user*/
        {
            var model = _studentFacade.GetStudent(User.Identity.Name).ProjectTo<StudentViewModel>().First();

            return PartialView(model);
        }

        [Route("FileWidget")]
        [HttpGet]
        public PartialViewResult FileWidget()
        {
            var models = _studentFacade.GetFilesByStudentEmail(User.Identity.Name)
                .ProjectTo<FileListViewModel>();

            return PartialView(models.ToArray());
        }

        [Route("EvidenceListWidget")] 
        [HttpGet]
        public PartialViewResult EvidenceListWidget() /* toont alle bewijzen */
        {
            var models = _studentFacade.GetEvidenceByStudentEmail(User.Identity.Name)
                .ProjectTo<EvidenceListViewModel>();

            return PartialView(models.ToArray());
        }

        [Route("EducationListWidget")]
        [HttpGet]
        public PartialViewResult EducationListWidget() /* lijst van alle modules + vakken */
        {
            var models = _studentFacade.GetPrevEducationsByStudentEmail(User.Identity.Name)
                .ProjectTo<EducationListViewModel>();

            return PartialView(models.ToArray());
        }

        [Route("AddFileWidget")]
        [HttpGet]
        public PartialViewResult AddFileWidget()
        {
            return PartialView();
        }

        [Route("AddEducationWidget")] /* voeg opleiding toe */
        [HttpGet]
        public PartialViewResult AddEducationWidget()
        {
            return PartialView();
        }

        [PreventSpam]
        [Route("AddEducation")]
        [HttpPost]
        public ActionResult AddEducation(AddEducationViewModel viewModel) /* voeg een vak toe */
        {

            ModelState.Clear();
            var validation = TryValidateModel(viewModel);
            var errors = (from modelstate in ModelState.Values from error in modelstate.Errors select error.ErrorMessage).ToList();

            if (!validation) return Json(errors.ToArray());

            _studentFacade.InsertPrevEducation(viewModel.Education, User.Identity.Name);
            errors.Add("Finish");
            return Json(errors.ToArray());
        }

        [Route("DeleteEducation")]
        [HttpPost]
        public ActionResult DeleteEducation(int educationId) /* verwijder een opleiding */
        {
            return Content(!_studentFacade.DeleteEducation(educationId)
                ? "gegeven opleiding kon niet verwijdert worden!"
                : "Voltooid!");
        }

        [PreventSpam(DelayRequest = 3600)]
        [Route("StudentSync")]
        public ActionResult StudentSync()
        {
            var configFile = new ConfigFile();
            if (ViewData.ModelState.IsValid) _studentFacade.SyncStudent(User.Identity.Name, configFile.AcademieJaar());
            return RedirectToAction("Index");
        }

        [Route("Submit")]
       
        public ActionResult SubmitFile()
        {
            var configFile = new ConfigFile();
            return Json(_studentFacade.SumbitFile(User.Identity.Name, configFile.AcademieJaar()), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region dossier
        [Route("File/{fileId}")]
        [HttpGet]
        public ActionResult File(int fileId) 
        {
            if (!_studentFacade.IsFileFromStudent(User.Identity.Name, fileId))
                return RedirectToAction("Index");

            return View();
        }

        [Route("RequestedPartimsWidget")]
        [HttpGet]
        public ActionResult RequestedPartimsWidget(int fileId)
        {
            if (!_studentFacade.IsFileFromStudent(User.Identity.Name, fileId))
                return RedirectToAction("Index");

            var models = _studentFacade.GetPartims(fileId, PartimMode.Requested)
                .ProjectTo<PartimViewModel>();
            
            return PartialView(models.ToArray());
        }

        [Route("AvailablePartimsWidget")]
        [HttpGet]
        public ActionResult AvailablePartimsWidget(int fileId)
        {
            if (!_studentFacade.IsFileFromStudent(User.Identity.Name, fileId))
                return RedirectToAction("Index");

            var models = _studentFacade.GetPartims(fileId, PartimMode.Available)
                .ProjectTo<PartimViewModel>();

            return PartialView(models.ToArray());
        }

        [Route("SelectEvidenceWidget")]
        [HttpGet]
        public PartialViewResult SelectEvidenceWidget(int fileId)
        {
            var models = _studentFacade.GetEvidenceByStudentEmail(User.Identity.Name)
                .ProjectTo<EvidenceListViewModel>();

            ViewBag.FileStatus = _studentFacade.GetFileStatus(fileId);

            return PartialView(models.ToArray());
        }

        [Route("SelectEducationWidget")]
        [HttpGet]
        public PartialViewResult SelectEducationWidget(int fileId)
        {
            var models =
                _studentFacade.GetPrevEducationsByStudentEmail(User.Identity.Name).ProjectTo<EducationListViewModel>();

            ViewBag.FileStatus = _studentFacade.GetFileStatus(fileId);

            return PartialView(models.ToArray());
        }

        [Route("RequestDetailWidget")]
        [HttpGet]
        public PartialViewResult RequestDetailWidget(int fileId)
        {
            var models = _studentFacade.GetRequestByFileId(fileId)
                .ProjectTo<RequestDetailViewModel>();

            return PartialView(models.ToArray());
        }

        [Route("NewFile")]
        [HttpGet]
        public ActionResult NewFile()
        {
            var configFile = new ConfigFile();
            var academieJaar = configFile.AcademieJaar();

            if(!_studentFacade.SyncStudentPartims(User.Identity.Name,academieJaar))
                return RedirectToAction("Index");

            var education = _studentFacade.GetEducation(User.Identity.Name);
            var dossier = new ServiceLayer.Student.Models.File
            {
                StudentMail = User.Identity.Name,
                DateCreated = DateTime.Now,
                Education = education,
                AcademicYear = academieJaar
            };

            var newId = _studentFacade.InsertFile(dossier);

            return RedirectToAction("Index");
        }

        [Route("AddAanvraag")]
        [HttpPost]
        public ActionResult AddAanvraag(AddRequestViewModel viewModel)
        {
            if (_studentFacade.IsFileFromStudent(User.Identity.Name, viewModel.FileId)) Content("Don't cheat!");
            return Content(_studentFacade.AddRequestInFile(viewModel.FileId,viewModel.Code));
        }

        [Route("SaveAanvraag")]
        [HttpPost]
        public ActionResult SaveAanvraag(RequestViewModel viewModel)
        {
            var requestId = int.Parse(viewModel.RequestId);
            if (_studentFacade.IsRequestFromStudent(viewModel.FileId, requestId, User.Identity.Name)) Content("Don't cheat!");

            var request = new Request
            {
                Id = requestId,
                FileId = viewModel.FileId
            }; ;
            if (viewModel.Evidence != null)
            {
                viewModel.Evidence = viewModel.Evidence.Distinct().ToArray();
                request.Evidence = viewModel.Evidence.Select(evidenceId => new Evidence
                {
                    Id = evidenceId
                    }).AsQueryable();
            }
            if (viewModel.Educations != null)
            {
                viewModel.Educations = viewModel.Educations.Distinct().ToArray();
                request.Educations = viewModel.Educations.Select(educationId => new PrevEducation
                {
                    Id = educationId
                }).AsQueryable();
            }


            return Content(!_studentFacade.SyncRequestInFile(request) ? "Don't cheat!" : "Saved!");
        }

        [Route("Delete")]
        [HttpPost]
        public ActionResult DeleteAanvraag(int fileId, string requestId)
        {
            var aanvraagId = int.Parse(requestId);
            return !_studentFacade.IsRequestFromStudent(fileId, aanvraagId, User.Identity.Name)
                ? Content("Don't cheat!")
                : Content(!_studentFacade.DeleteRequest(fileId, aanvraagId)
                    ? "RequestPartimInformation bestaat niet!"
                    : "Voltooid!");
        }
        #endregion
    }
}