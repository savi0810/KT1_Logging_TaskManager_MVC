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
    public class CurrentTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CurrentTasksController> _logger;

        public CurrentTasksController(ApplicationDbContext context, ILogger<CurrentTasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: CurrentTasks
        public async Task<IActionResult> Index()
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: получение списка текущих задач");

            try
            {
                await CheckOverdueTasks();

                var tasks = await _context.CurrentTasks
                    .OrderByDescending(t => t.CreatedDate)
                    .ToListAsync();

                sw.Stop();
                _logger.LogInformation("Получено {TaskCount} текущих задач за {ElapsedMs} мс", tasks.Count, sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: получение списка текущих задач (успешно)");

                return View(tasks);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при получении списка текущих задач за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: получение списка текущих задач (ошибка)");
                return View(new List<CurrentTasks>());
            }
        }

        // GET: CurrentTasks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: просмотр деталей задачи с ID {TaskId}", id);

            try
            {
                if (id == null)
                {
                    sw.Stop();
                    _logger.LogWarning("Попытка просмотра деталей задачи без указания ID (прошло {ElapsedMs} мс)", sw.ElapsedMilliseconds);
                    return NotFound();
                }

                var currentTasks = await _context.CurrentTasks
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (currentTasks == null)
                {
                    sw.Stop();
                    _logger.LogError("Задача с ID {TaskId} не найдена (прошло {ElapsedMs} мс)", id, sw.ElapsedMilliseconds);
                    return NotFound();
                }

                sw.Stop();
                _logger.LogInformation("Просмотр деталей задачи \"{TaskName}\" за {ElapsedMs} мс", currentTasks.TaskName, sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: просмотр деталей задачи (успешно)");
                return View(currentTasks);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при просмотре деталей задачи за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: просмотр деталей задачи (ошибка)");
                throw;
            }
        }

        // GET: CurrentTasks/Create
        public IActionResult Create()
        {
            _logger.LogDebug("Отображение формы создания задачи");
            return View();
        }

        // POST: CurrentTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TaskName,TaskDescription,DueDate,Priority")] CurrentTasks currentTasks)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: создание новой задачи");

            try
            {
                if (string.IsNullOrWhiteSpace(currentTasks.TaskName))
                {
                    sw.Stop();
                    _logger.LogWarning("Попытка создать задачу с пустым названием (прошло {ElapsedMs} мс)", sw.ElapsedMilliseconds);
                    ModelState.AddModelError("TaskName", "Название задачи обязательно.");
                    return View(currentTasks);
                }

                if (ModelState.IsValid)
                {
                    currentTasks.Id = Guid.NewGuid();
                    currentTasks.CreatedDate = DateTime.Now;
                    _context.Add(currentTasks);
                    await _context.SaveChangesAsync();

                    var taskCount = await _context.CurrentTasks.CountAsync();
                    sw.Stop();
                    _logger.LogInformation("Задача \"{TaskName}\" успешно добавлена за {ElapsedMs} мс. Теперь задач: {TaskCount}",
                        currentTasks.TaskName, sw.ElapsedMilliseconds, taskCount);
                    _logger.LogDebug("Окончание операции: создание задачи (успешно)");

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    sw.Stop();
                    _logger.LogWarning("Невалидные данные при создании задачи (прошло {ElapsedMs} мс)", sw.ElapsedMilliseconds);
                    _logger.LogDebug("Окончание операции: создание задачи (ошибка валидации)");
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при сохранении задачи за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: создание задачи (ошибка)");
                ModelState.AddModelError("", "Произошла ошибка при сохранении задачи.");
            }

            return View(currentTasks);
        }

        // GET: CurrentTasks/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: редактирование задачи (получение данных) с ID {TaskId}", id);

            try
            {
                if (id == null)
                {
                    sw.Stop();
                    _logger.LogWarning("Попытка редактирования задачи без указания ID (прошло {ElapsedMs} мс)", sw.ElapsedMilliseconds);
                    return NotFound();
                }

                var currentTasks = await _context.CurrentTasks.FindAsync(id);
                if (currentTasks == null)
                {
                    sw.Stop();
                    _logger.LogError("Задача с ID {TaskId} не найдена для редактирования (прошло {ElapsedMs} мс)", id, sw.ElapsedMilliseconds);
                    return NotFound();
                }

                sw.Stop();
                _logger.LogInformation("Задача \"{TaskName}\" загружена для редактирования за {ElapsedMs} мс", currentTasks.TaskName, sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: редактирование задачи (получение данных) - успешно");
                return View(currentTasks);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при загрузке задачи для редактирования за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: редактирование задачи (получение данных) - ошибка");
                throw;
            }
        }

        // POST: CurrentTasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,TaskName,TaskDescription,DueDate,Priority,CreatedDate")] CurrentTasks currentTasks)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: сохранение изменений задачи с ID {TaskId}", id);

            try
            {
                if (id != currentTasks.Id)
                {
                    sw.Stop();
                    _logger.LogWarning("Несоответствие ID при редактировании (прошло {ElapsedMs} мс)", sw.ElapsedMilliseconds);
                    return NotFound();
                }

                if (string.IsNullOrWhiteSpace(currentTasks.TaskName))
                {
                    sw.Stop();
                    _logger.LogWarning("Попытка сохранить задачу с пустым названием (прошло {ElapsedMs} мс)", sw.ElapsedMilliseconds);
                    ModelState.AddModelError("TaskName", "Название задачи обязательно.");
                    return View(currentTasks);
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(currentTasks);
                        await _context.SaveChangesAsync();
                        sw.Stop();
                        _logger.LogInformation("Задача \"{TaskName}\" успешно отредактирована за {ElapsedMs} мс",
                            currentTasks.TaskName, sw.ElapsedMilliseconds);
                        _logger.LogDebug("Окончание операции: сохранение изменений задачи (успешно)");
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CurrentTasksExists(currentTasks.Id))
                        {
                            sw.Stop();
                            _logger.LogError("Задача с ID {TaskId} не найдена при обновлении (прошло {ElapsedMs} мс)",
                                currentTasks.Id, sw.ElapsedMilliseconds);
                            return NotFound();
                        }
                        else
                        {
                            sw.Stop();
                            _logger.LogError("Ошибка конкурентного доступа при редактировании задачи ID: {TaskId} (прошло {ElapsedMs} мс)",
                                currentTasks.Id, sw.ElapsedMilliseconds);
                            throw;
                        }
                    }
                }
                else
                {
                    sw.Stop();
                    _logger.LogWarning("Невалидные данные при редактировании задачи (прошло {ElapsedMs} мс)", sw.ElapsedMilliseconds);
                    _logger.LogDebug("Окончание операции: сохранение изменений задачи (ошибка валидации)");
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при редактировании задачи за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: сохранение изменений задачи (ошибка)");
                ModelState.AddModelError("", "Произошла ошибка при сохранении изменений.");
            }

            return View(currentTasks);
        }

        // POST: CurrentTasks/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: удаление задачи с ID {TaskId}", id);

            try
            {
                var currentTasks = await _context.CurrentTasks.FindAsync(id);
                if (currentTasks != null)
                {
                    var deletedTask = new DeletedTasks
                    {
                        Id = Guid.NewGuid(),
                        TaskName = currentTasks.TaskName,
                        TaskDescription = currentTasks.TaskDescription,
                        DeletedDate = DateTime.Now
                    };

                    _context.DeletedTasks.Add(deletedTask);
                    _context.CurrentTasks.Remove(currentTasks);
                    await _context.SaveChangesAsync();

                    var currentCount = await _context.CurrentTasks.CountAsync();
                    var deletedCount = await _context.DeletedTasks.CountAsync();
                    sw.Stop();
                    _logger.LogInformation("Задача \"{TaskName}\" удалена за {ElapsedMs} мс. Теперь текущих задач: {CurrentCount}, удаленных: {DeletedCount}",
                        currentTasks.TaskName, sw.ElapsedMilliseconds, currentCount, deletedCount);
                    _logger.LogDebug("Окончание операции: удаление задачи (успешно)");
                }
                else
                {
                    sw.Stop();
                    _logger.LogError("Задача с ID {TaskId} не найдена для удаления (прошло {ElapsedMs} мс)", id, sw.ElapsedMilliseconds);
                    _logger.LogDebug("Окончание операции: удаление задачи (задача не найдена)");
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при удалении задачи за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: удаление задачи (ошибка)");
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: CurrentTasks/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(Guid id)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало операции: завершение задачи с ID {TaskId}", id);

            try
            {
                var currentTask = await _context.CurrentTasks.FindAsync(id);
                if (currentTask == null)
                {
                    sw.Stop();
                    _logger.LogError("Задача с ID {TaskId} не найдена для завершения (прошло {ElapsedMs} мс)", id, sw.ElapsedMilliseconds);
                    _logger.LogDebug("Окончание операции: завершение задачи (задача не найдена)");
                    return NotFound();
                }

                var completedTask = new CompletedTasks
                {
                    Id = Guid.NewGuid(),
                    TaskName = currentTask.TaskName,
                    TaskDescription = currentTask.TaskDescription,
                    CompletedDate = DateTime.Now
                };

                _context.CompletedTasks.Add(completedTask);
                _context.CurrentTasks.Remove(currentTask);
                await _context.SaveChangesAsync();

                var currentCount = await _context.CurrentTasks.CountAsync();
                var completedCount = await _context.CompletedTasks.CountAsync();
                sw.Stop();
                _logger.LogInformation("Задача \"{TaskName}\" завершена за {ElapsedMs} мс. Теперь текущих задач: {CurrentCount}, завершенных: {CompletedCount}",
                    currentTask.TaskName, sw.ElapsedMilliseconds, currentCount, completedCount);
                _logger.LogDebug("Окончание операции: завершение задачи (успешно)");
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при завершении задачи за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание операции: завершение задачи (ошибка)");
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task CheckOverdueTasks()
        {
            var sw = Stopwatch.StartNew();
            _logger.LogDebug("Начало проверки просроченных задач");

            try
            {
                var overdueTasks = await _context.CurrentTasks
                    .Where(t => t.DueDate.HasValue && t.DueDate.Value < DateTime.Now)
                    .ToListAsync();

                int movedToOverdue = 0;
                foreach (var task in overdueTasks)
                {
                    var alreadyOverdue = await _context.OverdueTasks
                        .AnyAsync(ot => ot.TaskName == task.TaskName && ot.TaskDescription == task.TaskDescription);

                    if (!alreadyOverdue)
                    {
                        var overdueTask = new OverdueTasks
                        {
                            Id = Guid.NewGuid(),
                            TaskName = task.TaskName,
                            TaskDescription = task.TaskDescription,
                            WhenOverdueDate = DateTime.Now
                        };

                        _context.OverdueTasks.Add(overdueTask);
                        _context.CurrentTasks.Remove(task);
                        movedToOverdue++;
                    }
                }

                if (overdueTasks.Any())
                {
                    await _context.SaveChangesAsync();
                }

                sw.Stop();
                _logger.LogInformation("Проверка просроченных задач: перемещено {MovedCount} задач за {ElapsedMs} мс",
                    movedToOverdue, sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание проверки просроченных задач (успешно)");
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Ошибка при проверке просроченных задач за {ElapsedMs} мс", sw.ElapsedMilliseconds);
                _logger.LogDebug("Окончание проверки просроченных задач (ошибка)");
            }
        }

        private bool CurrentTasksExists(Guid id)
        {
            return _context.CurrentTasks.Any(e => e.Id == id);
        }
    }
}