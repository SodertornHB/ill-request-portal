using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using IllRequestPortal.Logic.Services;
using IllRequestPortal.Web.ViewModel;
using Logic.Model;

namespace Common.Web.Controllers
{
    public partial class SettingController : Controller
    {
        private readonly ILogger<SettingController> logger;
        private readonly ISettingService service;
        private readonly IMapper mapper;

        public SettingController(ILogger<SettingController> logger, 
        ISettingService service, 
        IMapper mapper)
        {
            this.logger = logger;
            this.service = service;
            this.mapper = mapper;
        }
        [HttpGet]
        public virtual async Task<IActionResult> Index()
        {
            var list = await service.GetAll();
            var viewModels = mapper.Map<IEnumerable<SettingViewModel>>(list);
            return View(viewModels.OrderByDescending(x => x.Id));
        }
        
        public ActionResult Create()
        {
            return View(new SettingViewModel());
        }

        [HttpPost]
        public virtual async Task<ActionResult> Create([FromForm]SettingViewModel viewModel)
        {
            var model = mapper.Map<Setting>(viewModel);
            await service.Insert(model);
            return RedirectToAction(nameof(Index));
        }

        public virtual async Task<ActionResult> Edit(int id)
        {
            var entity = await service.Get(id);
            return View(mapper.Map<SettingViewModel>(entity));
        }


        [HttpPost]
        public virtual async Task<ActionResult> Edit([FromForm]SettingViewModel viewModel)
        {
            var model = mapper.Map<Setting>(viewModel);
            await service.Update(model);
            return RedirectToAction(nameof(Index));         
        }

        public virtual async Task<ActionResult> Remove(int id)
        {
            var entity = await service.Get(id);
            return View(mapper.Map<SettingViewModel>(entity));        
        }

        [HttpPost]
        public virtual async Task<ActionResult> Remove([FromForm]SettingViewModel viewModel)
        {
            var model = mapper.Map<Setting>(viewModel);
            await service.Delete(viewModel.Id);
            return RedirectToAction(nameof(Index));         
        }
    }
}