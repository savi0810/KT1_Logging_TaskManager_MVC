using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KT1_Logging_TaskManager_MVC.Models;

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
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: получение списка удаленных задач");

            try
            {
                var tasks = await _context.DeletedTasks
                    .OrderByDescending(t => t.DeletedDate)
                    .ToListAsync();

                sw.Stop();
                _logger.LogInformation("Получено {TaskCount} удаленных задач за {ElapsedMs} мс", tasks.Count, sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: получение списка удаленных задач (успешно)");
                return View(tasks);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при получении списка удаленных задач за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: получение списка удаленных задач (ошибка)");
                return View(new List<DeletedTasks>());
            }
        }

        // GET: DeletedTasks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: просмотр деталей удаленной задачи с ID {TaskId}", id);

            try
            {
                if (id == null)
                {
                    sw.Stop();
                    _logger.LogWarning("Попытка просмотра деталей удаленной задачи без указания ID (прошло {ElapsedMs} мс)", sw.ElapsedMilliseconds);
                    return NotFound();
                }

                var deletedTask = await _context.DeletedTasks
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (deletedTask == null)
                {
                    sw.Stop();
                    _logger.LogError("Удаленная задача с ID {TaskId} не найдена (прошло {ElapsedMs} мс)", id, sw.ElapsedMilliseconds);
                    return NotFound();
                }

                sw.Stop();
                _logger.LogInformation("Просмотр деталей удаленной задачи \"{TaskName}\" за {ElapsedMs} мс", deletedTask.TaskName, sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: просмотр деталей удаленной задачи (успешно)");
                return View(deletedTask);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при просмотре деталей удаленной задачи за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: просмотр деталей удаленной задачи (ошибка)");
                throw;
            }
        }

        // POST: DeletedTasks/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: окончательное удаление задачи с ID {TaskId} из корзины", id);

            try
            {
                var deletedTask = await _context.DeletedTasks.FindAsync(id);
                if (deletedTask != null)
                {
                    _context.DeletedTasks.Remove(deletedTask);
                    await _context.SaveChangesAsync();

                    var remainingCount = await _context.DeletedTasks.CountAsync();
                    sw.Stop();
                    _logger.LogInformation("Удаленная задача \"{TaskName}\" окончательно удалена за {ElapsedMs} мс. Теперь удаленных задач: {RemainingCount}",
                        deletedTask.TaskName, sw.ElapsedMilliseconds, remainingCount);
                    _logger.LogDebug("Окончание операции: окончательное удаление задачи (успешно)");
                }
                else
                {
                    sw.Stop();
                    _logger.LogError("Удаленная задача с ID {TaskId} не найдена для полного удаления (прошло {ElapsedMs} мс)", id, sw.ElapsedMilliseconds);
                    _logger.LogDebug("Окончание операции: окончательное удаление задачи (задача не найдена)");
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при окончательном удалении задачи за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: окончательное удаление задачи (ошибка)");
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: DeletedTasks/ClearAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAll()
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: очистка всех удаленных задач");

            try
            {
                var allTasks = await _context.DeletedTasks.ToListAsync();
                if (allTasks.Any())
                {
                    _context.DeletedTasks.RemoveRange(allTasks);
                    await _context.SaveChangesAsync();
                    sw.Stop();
                    _logger.LogInformation("Очищены все удаленные задачи. Удалено {TaskCount} задач за {ElapsedMs} мс", allTasks.Count, sw.ElapsedMilliseconds);
                }
                else
                {
                    sw.Stop();
                    _logger.LogWarning("Попытка очистки списка удаленных задач, но он пуст (прошло {ElapsedMs} мс)", sw.ElapsedMilliseconds);
                }
                _logger.LogDebug("Окончание операции: очистка всех удаленных задач (успешно)");
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при очистке удаленных задач за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: очистка всех удаленных задач (ошибка)");
                throw;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}