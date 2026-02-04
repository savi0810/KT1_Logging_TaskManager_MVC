using KT1_Logging_TaskManager_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KT1_Logging_TaskManager_MVC.Controllers
{
    public class DeletedTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeletedTasksController> _logger;

        public DeletedTasksController(ApplicationDbContext context, ILogger<DeletedTasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: DeletedTasks
        public async Task<IActionResult> Index()
        {
            _logger.LogDebug("[TRACE] Начало операции Index - получение списка удаленных задач");

            var tasks = await _context.DeletedTasks
                .OrderByDescending(t => t.DeletedDate)
                .ToListAsync();

            _logger.LogInformation("[INFO] Получено {TaskCount} удаленных задач", tasks.Count);
            _logger.LogDebug("[TRACE] Конец операции Index");
            return View(tasks);
        }

        // GET: DeletedTasks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            _logger.LogDebug("[TRACE] Начало операции Details для удаленной задачи ID: {TaskId}", id);

            if (id == null)
            {
                _logger.LogWarning("[WARN] Попытка просмотра деталей удаленной задачи без указания ID");
                return NotFound();
            }

            var deletedTask = await _context.DeletedTasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deletedTask == null)
            {
                _logger.LogError("[ERROR] Удаленная задача с ID {TaskId} не найдена", id);
                return NotFound();
            }

            _logger.LogInformation("[INFO] Просмотр деталей удаленной задачи: \"{TaskName}\"", deletedTask.TaskName);
            _logger.LogDebug("[TRACE] Конец операции Details");
            return View(deletedTask);
        }

        // POST: DeletedTasks/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogDebug("[TRACE] Начало операции Delete для удаленной задачи ID: {TaskId}", id);

            var deletedTask = await _context.DeletedTasks.FindAsync(id);
            if (deletedTask != null)
            {
                _context.DeletedTasks.Remove(deletedTask);
                await _context.SaveChangesAsync();

                var remainingCount = await _context.DeletedTasks.CountAsync();
                _logger.LogInformation("[INFO] Удаленная задача \"{TaskName}\" окончательно удалена", deletedTask.TaskName);
                _logger.LogInformation("[INFO] Теперь удаленных задач: {RemainingCount}", remainingCount);
                _logger.LogDebug("[TRACE] Конец операции Delete - успешно");
            }
            else
            {
                _logger.LogError("[ERROR] Удаленная задача с ID {TaskId} не найдена для полного удаления", id);
                _logger.LogDebug("[TRACE] Конец операции Delete - задача не найдена");
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: DeletedTasks/ClearAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAll()
        {
            _logger.LogDebug("[TRACE] Начало операции ClearAll - очистка всех удаленных задач");

            var allTasks = await _context.DeletedTasks.ToListAsync();
            if (allTasks.Any())
            {
                _context.DeletedTasks.RemoveRange(allTasks);
                await _context.SaveChangesAsync();
                _logger.LogInformation("[INFO] Очищены все удаленные задачи. Удалено: {TaskCount} задач", allTasks.Count);
            }
            else
            {
                _logger.LogWarning("[WARN] Попытка очистки списка удаленных задач, но он пуст");
            }

            _logger.LogDebug("[TRACE] Конец операции ClearAll");
            return RedirectToAction(nameof(Index));
        }
    }
}