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
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: получение списка просроченных задач");

            try
            {
                var tasks = await _context.OverdueTasks
                    .OrderByDescending(t => t.WhenOverdueDate)
                    .ToListAsync();

                sw.Stop();
                _logger.LogInformation("Получено {TaskCount} просроченных задач за {ElapsedMs} мс", tasks.Count, sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: получение списка просроченных задач (успешно)");
                return View(tasks);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при получении списка просроченных задач за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: получение списка просроченных задач (ошибка)");
                return View(new List<OverdueTasks>());
            }
        }

        // GET: OverdueTasks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: просмотр деталей просроченной задачи с ID {TaskId}", id);

            try
            {
                if (id == null)
                {
                    sw.Stop();
                    _logger.LogWarning("Попытка просмотра деталей просроченной задачи без указания ID (прошло {ElapsedMs} мс)", sw.ElapsedMilliseconds);
                    return NotFound();
                }

                var overdueTask = await _context.OverdueTasks
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (overdueTask == null)
                {
                    sw.Stop();
                    _logger.LogError("Просроченная задача с ID {TaskId} не найдена (прошло {ElapsedMs} мс)", id, sw.ElapsedMilliseconds);
                    return NotFound();
                }

                sw.Stop();
                _logger.LogInformation("Просмотр деталей просроченной задачи \"{TaskName}\" за {ElapsedMs} мс", overdueTask.TaskName, sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: просмотр деталей просроченной задачи (успешно)");
                return View(overdueTask);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при просмотре деталей просроченной задачи за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: просмотр деталей просроченной задачи (ошибка)");
                throw;
            }
        }

        // POST: OverdueTasks/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: удаление просроченной задачи с ID {TaskId}", id);

            try
            {
                var overdueTask = await _context.OverdueTasks.FindAsync(id);
                if (overdueTask != null)
                {
                    _context.OverdueTasks.Remove(overdueTask);
                    await _context.SaveChangesAsync();

                    var remainingCount = await _context.OverdueTasks.CountAsync();
                    sw.Stop();
                    _logger.LogInformation("Просроченная задача \"{TaskName}\" удалена за {ElapsedMs} мс. Теперь просроченных задач: {RemainingCount}",
                        overdueTask.TaskName, sw.ElapsedMilliseconds, remainingCount);
                    _logger.LogDebug("Окончание операции: удаление просроченной задачи (успешно)");
                }
                else
                {
                    sw.Stop();
                    _logger.LogError("Просроченная задача с ID {TaskId} не найдена для удаления (прошло {ElapsedMs} мс)", id, sw.ElapsedMilliseconds);
                    _logger.LogDebug("Окончание операции: удаление просроченной задачи (задача не найдена)");
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при удалении просроченной задачи за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: удаление просроченной задачи (ошибка)");
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: OverdueTasks/ClearAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAll()
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: очистка всех просроченных задач");

            try
            {
                var allTasks = await _context.OverdueTasks.ToListAsync();
                if (allTasks.Any())
                {
                    _context.OverdueTasks.RemoveRange(allTasks);
                    await _context.SaveChangesAsync();
                    sw.Stop();
                    _logger.LogInformation("Очищены все просроченные задачи. Удалено {TaskCount} задач за {ElapsedMs} мс", allTasks.Count, sw.ElapsedMilliseconds);
                }
                else
                {
                    sw.Stop();
                    _logger.LogWarning("Попытка очистки списка просроченных задач, но он пуст (прошло {ElapsedMs} мс)", sw.ElapsedMilliseconds);
                }
                _logger.LogDebug("Окончание операции: очистка всех просроченных задач (успешно)");
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при очистке просроченных задач за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: очистка всех просроченных задач (ошибка)");
                throw;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}