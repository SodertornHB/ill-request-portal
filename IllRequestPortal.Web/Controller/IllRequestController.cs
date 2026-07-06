using AutoMapper;
using IllRequestPortal.Logic.Model;
using IllRequestPortal.Logic.Services;
using IllRequestPortal.Web.ViewModel;
using Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IllRequestPortal.Web.Controllers
{
    public partial class IllRequestController : Controller
    {
        private readonly ILogger<IllRequestController> logger;
        private readonly IIllRequestService service;
        private readonly IMapper mapper;
        private readonly ISettingService settingService;
        private readonly LocService locService;

        public IllRequestController(ILogger<IllRequestController> logger,
        IIllRequestService service,
        IMapper mapper,
        ISettingService settingService,
        LocService locService)
        {
            this.logger = logger;
            this.service = service;
            this.mapper = mapper;
            this.settingService = settingService;
            this.locService = locService;
        }

        public virtual async Task<IActionResult> Index()
        {
            var list = await service.GetAll();
            var viewModels = mapper.Map<IEnumerable<IllRequestViewModel>>(list.OrderByDescending(x => x.CreatedOn));
            return View(viewModels.OrderByDescending(x => x.Id));
        }

        public async Task<ActionResult> Create()
        {
            var settings = await settingService.GetAll();

            var closedPeriodStart = settings.FirstOrDefault(x => x.Key == "FormClosedStart");
            var closedPeriodEnd = settings.FirstOrDefault(x => x.Key == "FormClosedEnd");

            if (closedPeriodStart != null && closedPeriodEnd != null)
            {


                DateTime.TryParse(closedPeriodStart.Value, out DateTime startDate);
                DateTime.TryParse(closedPeriodEnd.Value, out DateTime endDate);
                if (FormClosedForPeriod(startDate, endDate))
                {
                    return View(nameof(FormClosed), new FeedbackViewModel
                    {
                        Message = String.Format(locService.GetLocalizedHtmlString("FormClosed"),
                startDate.ToString("d MMMM"),  // {0}
                endDate.ToString("d MMMM")     // {1}
                    )
                    });
                }
            }
            return View(new CreateIllRequestViewModel());
        }


        public ActionResult FormClosed()
        {
            return View();
        }

        [HttpPost]
        public virtual async Task<ActionResult> Create([FromForm] CreateIllRequestViewModel viewModel)
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

        #region private
        private bool FormClosedForPeriod(DateTime start, DateTime end)
        {
            //closed for longer period. Do not forget to change the SharedResource res file for the value FormClosed to match with the interval below.
            var now = DateTime.Now;
            return now >= start && now <= end;
        }
        #endregion private
    }
}