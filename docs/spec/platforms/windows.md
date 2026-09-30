# Платформа Windows (WIN)

## Компоненты

| Компонент | Проект | Роль |
|-----------|--------|------|
| UI | `FocusManager.App` (WinUI 3) | Включение режима, редактирование белых списков |
| Агент | `FocusManager.Agent` (Worker + WinForms tray) | Мониторинг событий, блокировки, хранение настроек |
| IPC | `FocusManager.Contracts` | Сообщения UI ↔ агент по named pipe `FocusManager.Agent.Pipe` (JSON, одна строка на запрос/ответ) |
| Интеграции с ОС | `FocusManager.Platform.Windows` | WMI, COM Проводника, реестр политик Chrome |

## Требования

### WIN-001 Агент с правами администратора
Агент запускается с правами администратора (манифест `requireAdministrator`): мониторинг запуска
процессов через WMI (`Win32_ProcessStartTrace`) требует повышенных прав.

Платформы: Windows — done

### WIN-002 Доступ к IPC
Named pipe агента доступен только текущему пользователю, администраторам и SYSTEM.

Платформы: Windows — done

### WIN-003 Игнорируемые процессы
Не блокируются: процессы других сессий Windows; процессы из каталога Windows и `Program Files\WindowsApps`;
процессы без полного пути; известные системные процессы (`explorer.exe`, `svchost.exe`, `conhost.exe`, …);
сам FocusManager (`focusmanager.agent.exe`, `focusmanager.app.exe`). Полный список — `AppEnforcer`.

Платформы: Windows — done

### WIN-004 Способ блокировки приложения
Запрещённый процесс завершается вместе с деревом дочерних процессов.

Платформы: Windows — done

### WIN-005 Способ блокировки сайтов
Для Chrome в `HKCU\Software\Policies\Google\Chrome` записываются `URLBlocklist = ["*"]` и
`URLAllowlist` из белого списка сайтов. При выключении режима обе политики очищаются.

Платформы: Windows — done
> ❓ Вопрос: политика в `HKCU` легко снимается пользователем. Нужна ли `HKLM` (агент и так администратор)?

### WIN-006 Хранилище
Настройки и белые списки хранятся в SQLite: `%LOCALAPPDATA%\FocusManager\focusmanager.db`
(для пользователя, под которым запущен агент).

Платформы: Windows — done

### WIN-007 Значок в трее
Агент показывает значок в трее с переключателем режима учёбы и пунктом «Выход».

Платформы: Windows — done
> ❓ Вопрос: «Выход» из трея позволяет обойти режим учёбы. Это допустимо?

### WIN-008 Скрипт запуска для разработки
`Start-FocusManager.cmd` собирает `FocusManager.Windows.slnf`, перезапускает агента с повышением прав
и запускает UI.

Платформы: Windows — done
