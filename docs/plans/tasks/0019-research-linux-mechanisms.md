---
id: 0019
title: Исследовать механизмы блокировки на Linux
status: todo
type: research
platform: linux
milestone: M4
priority: medium
depends_on: []
spec: [LNX-001, LNX-002, LNX-003, APP-001, SITE-005, FLD-001, CNT-003, GEN-006]
adr: [0007]
---

## Контекст

`docs/spec/platforms/linux.md` — черновик с гипотезами. Linux — основная платформа с тем же
функционалом, что Windows, включая блокировку папок (решения B1, B5).

## Что сделать

Прототипами на популярных дистрибутивах (Ubuntu LTS, Fedora; GNOME и KDE; X11 и Wayland) проверить:
1. Обнаружение запуска процессов (netlink proc connector, eBPF, опрос `/proc`) и их завершение;
   идентификация приложений из Flatpak и Snap.
2. Политики браузеров на Chromium и Firefox, в т. ч. для Flatpak/Snap-версий.
3. Блокировку папок: `fanotify` (запрет открытия) или интеграция с Nautilus/Dolphin; что возможно на Wayland.
4. Скрытие уведомлений других приложений (режим «Не беспокоить» GNOME/KDE).
5. Агент как служба systemd с root, IPC через Unix socket с доступом только для пользователя.
6. Форматы распространения (deb, rpm, Flatpak, AppImage) с учётом службы с правами root.

## Критерии приёмки

- [ ] ADR «Механизмы блокировки на Linux»; `platforms/linux.md` переведён из черновика в требования.
- [ ] Обновлена задача 0020 (или созданы задачи) на реализацию.

## Вне рамок

- Код в `src/Linux`.

## Результат
