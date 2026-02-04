using KT1_Logging_TaskManager_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KT1_Logging_TaskManager_MVC.Controllers
{
    public class OverdueTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OverdueTasksController> _logger;

        public OverdueTasksController(ApplicationDbContext context, ILogger<OverdueTasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: OverdueTasks
        public async Task<IActionResult> Index()
        {
            _logger.LogDebug("[TRACE] Начало операции Index - получение списка просроченных задач");

            var tasks = await _context.OverdueTasks
                .OrderByDescending(t => t.WhenOverdueDate)
                .ToListAsync();

            _logger.LogInformation("[INFO] Получено {TaskCount} просроченных задач", tasks.Count);
            _logger.LogDebug("[TRACE] Конец операции Index");
            return View(tasks);
        }

        // GET: OverdueTasks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            _logger.LogDebug("[TRACE] Начало операции Details для просроченной задачи ID: {TaskId}", id);

            if (id == null)
            {
                _logger.LogWarning("[WARN] Попытка просмотра деталей просроченной задачи без указания ID");
                return NotFound();
            }

            var overdueTask = await _context.OverdueTasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (overdueTask == null)
            {
                _logger.LogError("[ERROR] Просроченная задача с ID {TaskId} не найдена", id);
                return NotFound();
            }

            _logger.LogInformation("[INFO] Просмотр деталей просроченной задачи: \"{TaskName}\"", overdueTask.TaskName);
            _logger.LogDebug("[TRACE] Конец операции Details");
            return View(overdueTask);
        }

        // POST: OverdueTasks/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogDebug("[TRACE] Начало операции Delete для просроченной задачи ID: {TaskId}", id);

            var overdueTask = await _context.OverdueTasks.FindAsync(id);
            if (overdueTask != null)
            {
                _context.OverdueTasks.Remove(overdueTask);
                await _context.SaveChangesAsync();

                var remainingCount = await _context.OverdueTasks.CountAsync();
                _logger.LogInformation("[INFO] Просроченная задача \"{TaskName}\" удалена", overdueTask.TaskName);
                _logger.LogInformation("[INFO] Теперь просроченных задач: {RemainingCount}", remainingCount);
                _logger.LogDebug("[TRACE] Конец операции Delete - успешно");
            }
            else
            {
                _logger.LogError("[ERROR] Просроченная задача с ID {TaskId} не найдена для удаления", id);
                _logger.LogDebug("[TRACE] Конец операции Delete - задача не найдена");
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: OverdueTasks/ClearAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAll()
        {
            _logger.LogDebug("[TRACE] Начало операции ClearAll - очистка всех просроченных задач");

            var allTasks = await _context.OverdueTasks.ToListAsync();
            if (allTasks.Any())
            {
                _context.OverdueTasks.RemoveRange(allTasks);
                await _context.SaveChangesAsync();
                _logger.LogInformation("[INFO] Очищены все просроченные задачи. Удалено: {TaskCount} задач", allTasks.Count);
            }
            else
            {
                _logger.LogWarning("[WARN] Попытка очистки списка просроченных задач, но он пуст");
            }

            _logger.LogDebug("[TRACE] Конец операции ClearAll");
            return RedirectToAction(nameof(Index));
        }
    }
}