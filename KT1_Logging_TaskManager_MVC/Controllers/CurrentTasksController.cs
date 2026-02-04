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
            _logger.LogDebug("[TRACE] Начало операции Index - получение списка текущих задач");
            await CheckOverdueTasks();

            var tasks = await _context.CurrentTasks
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            _logger.LogInformation("[INFO] Получено {TaskCount} текущих задач", tasks.Count);
            _logger.LogDebug("[TRACE] Конец операции Index");
            return View(tasks);
        }

        // GET: CurrentTasks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            _logger.LogDebug("[TRACE] Начало операции Details для задачи ID: {TaskId}", id);

            if (id == null)
            {
                _logger.LogWarning("[WARN] Попытка просмотра деталей задачи без указания ID");
                return NotFound();
            }

            var currentTasks = await _context.CurrentTasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (currentTasks == null)
            {
                _logger.LogError("[ERROR] Задача с ID {TaskId} не найдена", id);
                return NotFound();
            }

            _logger.LogInformation("[INFO] Просмотр деталей задачи: \"{TaskName}\"", currentTasks.TaskName);
            _logger.LogDebug("[TRACE] Конец операции Details");
            return View(currentTasks);
        }

        // GET: CurrentTasks/Create
        public IActionResult Create()
        {
            _logger.LogDebug("[TRACE] Отображение формы создания задачи");
            return View();
        }

        // POST: CurrentTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TaskName,TaskDescription,DueDate,Priority")] CurrentTasks currentTasks)
        {
            _logger.LogDebug("[TRACE] Начало операции Create (POST) - создание новой задачи");
            _logger.LogDebug("[TRACE] Проверка введенных данных: Название='{TaskName}'", currentTasks.TaskName);

            if (string.IsNullOrWhiteSpace(currentTasks.TaskName))
            {
                _logger.LogWarning("[WARN] Попытка создать задачу с пустым названием");
                ModelState.AddModelError("TaskName", "Название задачи обязательно.");
                return View(currentTasks);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    currentTasks.Id = Guid.NewGuid();
                    currentTasks.CreatedDate = DateTime.Now;
                    _context.Add(currentTasks);
                    await _context.SaveChangesAsync();

                    var taskCount = await _context.CurrentTasks.CountAsync();
                    _logger.LogInformation("[INFO] Задача \"{TaskName}\" успешно добавлена", currentTasks.TaskName);
                    _logger.LogInformation("[INFO] Теперь текущих задач: {TaskCount}", taskCount);
                    _logger.LogDebug("[TRACE] Конец операции Create - успешно");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[ERROR] Ошибка при сохранении задачи");
                    ModelState.AddModelError("", "Произошла ошибка при сохранении задачи.");
                    _logger.LogDebug("[TRACE] Конец операции Create - ошибка");
                }
            }
            else
            {
                _logger.LogWarning("[WARN] Невалидные данные при создании задачи");
            }

            return View(currentTasks);
        }

        // GET: CurrentTasks/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            _logger.LogDebug("[TRACE] Начало операции Edit (GET) для задачи ID: {TaskId}", id);

            if (id == null)
            {
                _logger.LogWarning("[WARN] Попытка редактирования задачи без указания ID");
                return NotFound();
            }

            var currentTasks = await _context.CurrentTasks.FindAsync(id);
            if (currentTasks == null)
            {
                _logger.LogError("[ERROR] Задача с ID {TaskId} не найдена для редактирования", id);
                return NotFound();
            }

            _logger.LogInformation("[INFO] Редактирование задачи: \"{TaskName}\"", currentTasks.TaskName);
            _logger.LogDebug("[TRACE] Конец операции Edit (GET)");
            return View(currentTasks);
        }

        // POST: CurrentTasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,TaskName,TaskDescription,DueDate,Priority,CreatedDate")] CurrentTasks currentTasks)
        {
            _logger.LogDebug("[TRACE] Начало операции Edit (POST) для задачи ID: {TaskId}", id);

            if (id != currentTasks.Id)
            {
                _logger.LogWarning("[WARN] Несоответствие ID при редактировании. Ожидался {ExpectedId}, получен {ActualId}",
                    id, currentTasks.Id);
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(currentTasks.TaskName))
            {
                _logger.LogWarning("[WARN] Попытка сохранить задачу с пустым названием");
                ModelState.AddModelError("TaskName", "Название задачи обязательно.");
                return View(currentTasks);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(currentTasks);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("[INFO] Задача \"{TaskName}\" успешно отредактирована", currentTasks.TaskName);
                    _logger.LogDebug("[TRACE] Конец операции Edit - успешно");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CurrentTasksExists(currentTasks.Id))
                    {
                        _logger.LogError("[ERROR] Задача с ID {TaskId} не найдена при обновлении", currentTasks.Id);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError("[ERROR] Ошибка конкурентного доступа при редактировании задачи ID: {TaskId}",
                            currentTasks.Id);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[ERROR] Ошибка при редактировании задачи");
                    ModelState.AddModelError("", "Произошла ошибка при сохранении изменений.");
                    _logger.LogDebug("[TRACE] Конец операции Edit - ошибка");
                }
            }
            else
            {
                _logger.LogWarning("[WARN] Невалидные данные при редактировании задачи");
            }

            return View(currentTasks);
        }

        // POST: CurrentTasks/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogDebug("[TRACE] Начало операции Delete для задачи ID: {TaskId}", id);

            var currentTasks = await _context.CurrentTasks.FindAsync(id);
            if (currentTasks != null)
            {
                var deletedtedTask = new DeletedTasks
                {
                    Id = Guid.NewGuid(),
                    TaskName = currentTasks.TaskName,
                    TaskDescription = currentTasks.TaskDescription,
                    DeletedDate = DateTime.Now
                };

                _context.DeletedTasks.Add(deletedtedTask);
                _context.CurrentTasks.Remove(currentTasks);

                await _context.SaveChangesAsync();

                var currentCount = await _context.CurrentTasks.CountAsync();
                var deletedCount = await _context.DeletedTasks.CountAsync();

                _logger.LogInformation("[INFO] Задача \"{TaskName}\" удалена", currentTasks.TaskName);
                _logger.LogInformation("[INFO] Теперь текущих задач: {CurrentCount}, удаленных задач: {DeletedCount}",
                    currentCount, deletedCount);
                _logger.LogDebug("[TRACE] Конец операции Delete - успешно");
            }
            else
            {
                _logger.LogError("[ERROR] Задача с ID {TaskId} не найдена для удаления", id);
                _logger.LogDebug("[TRACE] Конец операции Delete - задача не найдена");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(Guid id)
        {
            _logger.LogDebug("[TRACE] Начало операции Complete для задачи ID: {TaskId}", id);

            var currentTask = await _context.CurrentTasks.FindAsync(id);
            if (currentTask == null)
            {
                _logger.LogError("[ERROR] Задача с ID {TaskId} не найдена для завершения", id);
                _logger.LogDebug("[TRACE] Конец операции Complete - задача не найдена");
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

            _logger.LogInformation("[INFO] Задача \"{TaskName}\" завершена", currentTask.TaskName);
            _logger.LogInformation("[INFO] Теперь текущих задач: {CurrentCount}, завершенных задач: {CompletedCount}",
                currentCount, completedCount);
            _logger.LogDebug("[TRACE] Конец операции Complete - успешно");

            return RedirectToAction(nameof(Index));
        }

        private async Task CheckOverdueTasks()
        {
            _logger.LogDebug("[TRACE] Начало проверки просроченных задач");

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

                    _logger.LogInformation("[INFO] Задача \"{TaskName}\" перемещена в просроченные (срок: {DueDate})",
                        task.TaskName, task.DueDate?.ToString("dd.MM.yyyy"));
                }
            }

            if (overdueTasks.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("[INFO] Перемещено в просроченные: {MovedCount} задач", movedToOverdue);
            }

            _logger.LogDebug("[TRACE] Конец проверки просроченных задач. Перемещено: {MovedCount}", movedToOverdue);
        }

        private bool CurrentTasksExists(Guid id)
        {
            return _context.CurrentTasks.Any(e => e.Id == id);
        }
    }
}