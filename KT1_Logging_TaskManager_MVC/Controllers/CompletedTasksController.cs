using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KT1_Logging_TaskManager_MVC;
using KT1_Logging_TaskManager_MVC.Models;

namespace KT1_Logging_TaskManager_MVC.Controllers
{
    public class CompletedTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompletedTasksController> _logger;

        public CompletedTasksController(ApplicationDbContext context, ILogger<CompletedTasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: CompletedTasks
        public async Task<IActionResult> Index()
        {
            _logger.LogDebug("[TRACE] Начало операции Index - получение списка завершенных задач");

            var tasks = await _context.CompletedTasks
                            .OrderByDescending(t => t.CompletedDate)
                            .ToListAsync();

            _logger.LogInformation("[INFO] Получено {TaskCount} завершенных задач", tasks.Count);
            _logger.LogDebug("[TRACE] Конец операции Index");
            return View(tasks);
        }

        // GET: CompletedTasks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            _logger.LogDebug("[TRACE] Начало операции Details для завершенной задачи ID: {TaskId}", id);

            if (id == null)
            {
                _logger.LogWarning("[WARN] Попытка просмотра деталей завершенной задачи без указания ID");
                return NotFound();
            }

            var completedTasks = await _context.CompletedTasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (completedTasks == null)
            {
                _logger.LogError("[ERROR] Завершенная задача с ID {TaskId} не найдена", id);
                return NotFound();
            }

            _logger.LogInformation("[INFO] Просмотр деталей завершенной задачи: \"{TaskName}\"", completedTasks.TaskName);
            _logger.LogDebug("[TRACE] Конец операции Details");
            return View(completedTasks);
        }

        // POST: CompletedTasks/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogDebug("[TRACE] Начало операции Delete для завершенной задачи ID: {TaskId}", id);

            var completedTasks = await _context.CompletedTasks.FindAsync(id);
            if (completedTasks != null)
            {
                _context.CompletedTasks.Remove(completedTasks);
                await _context.SaveChangesAsync();

                var remainingCount = await _context.CompletedTasks.CountAsync();
                _logger.LogInformation("[INFO] Завершенная задача \"{TaskName}\" удалена", completedTasks.TaskName);
                _logger.LogInformation("[INFO] Теперь завершенных задач: {RemainingCount}", remainingCount);
                _logger.LogDebug("[TRACE] Конец операции Delete - успешно");
            }
            else
            {
                _logger.LogError("[ERROR] Завершенная задача с ID {TaskId} не найдена для удаления", id);
                _logger.LogDebug("[TRACE] Конец операции Delete - задача не найдена");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAll()
        {
            _logger.LogDebug("[TRACE] Начало операции ClearAll - очистка всех завершенных задач");

            var allTasks = await _context.CompletedTasks.ToListAsync();
            if (allTasks.Any())
            {
                _context.CompletedTasks.RemoveRange(allTasks);
                await _context.SaveChangesAsync();
                _logger.LogInformation("[INFO] Очищены все завершенные задачи. Удалено: {TaskCount} задач", allTasks.Count);
            }
            else
            {
                _logger.LogWarning("[WARN] Попытка очистки списка завершенных задач, но он пуст");
            }

            _logger.LogDebug("[TRACE] Конец операции ClearAll");
            return RedirectToAction(nameof(Index));
        }
    }
}