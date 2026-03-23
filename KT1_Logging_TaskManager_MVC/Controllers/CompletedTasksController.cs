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
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: получение списка завершенных задач");

            try
            {
                var tasks = await _context.CompletedTasks
                    .OrderByDescending(t => t.CompletedDate)
                    .ToListAsync();

                sw.Stop();
                _logger.LogInformation("Получено {TaskCount} завершенных задач за {ElapsedMs} мс", tasks.Count, sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: получение списка завершенных задач (успешно)");
                return View(tasks);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при получении списка завершенных задач за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: получение списка завершенных задач (ошибка)");
                return View(new List<CompletedTasks>());
            }
        }

        // GET: CompletedTasks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: просмотр деталей завершенной задачи с ID {TaskId}", id);

            try
            {
                if (id == null)
                {
                    sw.Stop();
                    _logger.LogWarning("Попытка просмотра деталей завершенной задачи без указания ID (прошло {ElapsedMs} мс)", sw.ElapsedMilliseconds);
                    return NotFound();
                }

                var completedTasks = await _context.CompletedTasks
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (completedTasks == null)
                {
                    sw.Stop();
                    _logger.LogError("Завершенная задача с ID {TaskId} не найдена (прошло {ElapsedMs} мс)", id, sw.ElapsedMilliseconds);
                    return NotFound();
                }

                sw.Stop();
                _logger.LogInformation("Просмотр деталей завершенной задачи \"{TaskName}\" за {ElapsedMs} мс", completedTasks.TaskName, sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: просмотр деталей завершенной задачи (успешно)");
                return View(completedTasks);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при просмотре деталей завершенной задачи за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: просмотр деталей завершенной задачи (ошибка)");
                throw;
            }
        }

        // POST: CompletedTasks/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: удаление завершенной задачи с ID {TaskId}", id);

            try
            {
                var completedTasks = await _context.CompletedTasks.FindAsync(id);
                if (completedTasks != null)
                {
                    _context.CompletedTasks.Remove(completedTasks);
                    await _context.SaveChangesAsync();

                    var remainingCount = await _context.CompletedTasks.CountAsync();
                    sw.Stop();
                    _logger.LogInformation("Завершенная задача \"{TaskName}\" удалена за {ElapsedMs} мс. Теперь завершенных задач: {RemainingCount}",
                        completedTasks.TaskName, sw.ElapsedMilliseconds, remainingCount);
                    _logger.LogDebug("Окончание операции: удаление завершенной задачи (успешно)");
                }
                else
                {
                    sw.Stop();
                    _logger.LogError("Завершенная задача с ID {TaskId} не найдена для удаления (прошло {ElapsedMs} мс)", id, sw.ElapsedMilliseconds);
                    _logger.LogDebug("Окончание операции: удаление завершенной задачи (задача не найдена)");
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при удалении завершенной задачи за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: удаление завершенной задачи (ошибка)");
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: CompletedTasks/ClearAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAll()
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: очистка всех завершенных задач");

            try
            {
                var allTasks = await _context.CompletedTasks.ToListAsync();
                if (allTasks.Any())
                {
                    _context.CompletedTasks.RemoveRange(allTasks);
                    await _context.SaveChangesAsync();
                    sw.Stop();
                    _logger.LogInformation("Очищены все завершенные задачи. Удалено {TaskCount} задач за {ElapsedMs} мс", allTasks.Count, sw.ElapsedMilliseconds);
                }
                else
                {
                    sw.Stop();
                    _logger.LogWarning("Попытка очистки списка завершенных задач, но он пуст (прошло {ElapsedMs} мс)", sw.ElapsedMilliseconds);
                }
                _logger.LogDebug("Окончание операции: очистка всех завершенных задач (успешно)");
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при очистке завершенных задач за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: очистка всех завершенных задач (ошибка)");
                throw;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}