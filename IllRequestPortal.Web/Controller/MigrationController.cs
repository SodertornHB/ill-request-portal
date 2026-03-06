
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
    public partial class MigrationController : Controller
    {
        private readonly ILogger<MigrationController> logger;
        private readonly IMigrationService service;
        private readonly IMapper mapper;

        public MigrationController(ILogger<MigrationController> logger, 
        IMigrationService service, 
        IMapper mapper)
        {
            this.logger = logger;
            this.service = service;
            this.mapper = mapper;
        }

        public virtual async Task<IActionResult> Index()
        {
            var list = await service.GetAll();
            var viewModels = mapper.Map<IEnumerable<MigrationViewModel>>(list);
            return View(viewModels.OrderByDescending(x => x.Id));
        }
        
        public ActionResult Create()
        {
            return View(new MigrationViewModel());
        }

        [HttpPost]
        public virtual async Task<ActionResult> Create([FromForm]MigrationViewModel viewModel)
        {
            var model = mapper.Map<Migration>(viewModel);
            await service.Insert(model);
            return RedirectToAction(nameof(Index));
        }

        public virtual async Task<ActionResult> Edit(int id)
        {
            var entity = await service.Get(id);
            return View(mapper.Map<MigrationViewModel>(entity));
        }


        [HttpPost]
        public virtual async Task<ActionResult> Edit([FromForm]MigrationViewModel viewModel)
        {
            var model = mapper.Map<Migration>(viewModel);
            await service.Update(model);
            return RedirectToAction(nameof(Index));         
        }

        public virtual async Task<ActionResult> Remove(int id)
        {
            var entity = await service.Get(id);
            return View(mapper.Map<MigrationViewModel>(entity));        
        }

        [HttpPost]
        public virtual async Task<ActionResult> Remove([FromForm]MigrationViewModel viewModel)
        {
            var model = mapper.Map<Migration>(viewModel);
            await service.Delete(viewModel.Id);
            return RedirectToAction(nameof(Index));         
        }
    }
}