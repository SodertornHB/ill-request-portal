
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//--------------------------------------------------------------------------------------------------------------------

using AutoMapper;
using IllRequestPortal.Logic.Model;
using IllRequestPortal.Logic.Services;
using IllRequestPortal.Web.ViewModel;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IllRequestPortal.Web.Controllers
{
    public partial class IllRequestController : Controller
    {
        private readonly ILogger<IllRequestController> logger;
        private readonly IIllRequestService service;
        private readonly IMapper mapper;

        public IllRequestController(ILogger<IllRequestController> logger,
        IIllRequestService service,
        IMapper mapper)
        {
            this.logger = logger;
            this.service = service;
            this.mapper = mapper;
        }

        public virtual async Task<IActionResult> Index()
        {
            var list = await service.GetAll();
            var viewModels = mapper.Map<IEnumerable<IllRequestViewModel>>(list);
            return View(viewModels.OrderByDescending(x => x.Id));
        }

        public ActionResult Create()
        {
            return View(new IllRequestViewModelExtended());
        }

        [HttpPost]
        public virtual async Task<ActionResult> Create([FromForm] IllRequestViewModelExtended viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(viewModel);

                IllRequest model = mapper.Map<IllRequestExtended>(viewModel);

                await service.Insert(model);

                return View("Feedback", new FeedbackViewModel
                {
                    PageTitle = "ThankYouPageTitle",
                    Header = "ThankYouHeader",
                    Message = "ThankYouMessage"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while creating a new request.");

                ModelState.AddModelError(string.Empty, "Something went wrong while saving the request.");

                return View(viewModel);
            }
        }

        public virtual async Task<ActionResult> Edit(int id)
        {
            var entity = await service.Get(id);
            return View(mapper.Map<IllRequestViewModel>(entity));
        }


        [HttpPost]
        public virtual async Task<ActionResult> Edit([FromForm] IllRequestViewModel viewModel)
        {
            var model = mapper.Map<IllRequest>(viewModel);
            await service.Update(model);
            return RedirectToAction(nameof(Index));
        }

        public virtual async Task<ActionResult> Remove(int id)
        {
            var entity = await service.Get(id);
            return View(mapper.Map<IllRequestViewModel>(entity));
        }

        [HttpPost]
        public virtual async Task<ActionResult> Remove([FromForm] IllRequestViewModel viewModel)
        {
            var model = mapper.Map<IllRequest>(viewModel);
            await service.Delete(viewModel.Id);
            return RedirectToAction(nameof(Index));
        }
    }
}