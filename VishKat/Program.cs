using Microsoft.Extensions.DependencyInjection;
using VishKat.Application.Interfaces;
using VishKat.Application.Services;
using VishKat.Infrastructure.Interfaces;
using VishKat.Infrastructure.Repositories;
using VishKat.Domain.Implementations;
using VishKat.Domain.Interfaces;
using VishKat.Domain.Services;


var services = new ServiceCollection();

// Регистрируем бизнес-логику
services.AddTransient<IServiceCenter, ServiceCenter>();
services.AddTransient<IBeginnerPolicy, BeginnerPolicy>();

services.AddSingleton<IKickSharingService, KickSharingService>();
services.AddSingleton<IInputRep, InputRep>();
services.AddSingleton<IOutputRep, OutputRep>();

// Собираем контейнер
using var serviceProvider = services.BuildServiceProvider();

// Запрашиваем готовые объекты (контейнер сам создаст всё необходимое)
var parkService = serviceProvider.GetRequiredService<IKickSharingService>();
var input = serviceProvider.GetRequiredService<IInputRep>();
var output = serviceProvider.GetRequiredService<IOutputRep>();

var park = parkService.CreateKickSharing();


output.PrintMessage("=== Система управления парком VishKat запущена ===");

while (true)
{
    output.PrintMessage("\nДоступные действия:");
    output.PrintMessage("1. Добавить транспорта (Vehicle)");
    output.PrintMessage("2. Добавить вещь (Thing)");
    output.PrintMessage("3. Показать весь инвентарь");
    output.PrintMessage("4. Показать транспорт для новичков");
    output.PrintMessage("5. Показать сводку (количество и потребление энергии)");
    output.PrintMessage("0. Выход");

    // Используем наш безопасный ввод!
    int? choice = input.GetNumber("Выберите пункт меню: ");

    if (choice is null) 
    {
        // Пользователь ввёл белиберду, цикл начнётся заново
        continue;
    }

    if (choice == 0)
    {
        output.PrintMessage("Завершение работы программы...");
        break;
    }

    switch (choice)
    {
        case 1:
        {
            output.PrintMessage("\nВыберите категорию транспорта:");
            output.PrintMessage("1. Энергопотребляющий (ElectricBike, ElectricalScooter)");
            output.PrintMessage("2. Механический (Bike)");
            int? energyChoice = input.GetNumber("Выберите пункт меню: ");

            if (energyChoice is null) break;

            string name = input.GetLine("Введите название транспорта: ");
            int? id = input.GetNumber("Введите инвентарный номер: ");
            int? ease = input.GetNumber("Введите простоту управления (1-10): ");

            if (id is null || ease is null)
            {
                output.PrintMessage("Ошибка: некорректный ввод чисел.");
                break;
            }

            Vehicle? newVehicle = null;

            if (energyChoice == 1) // Для ElectricBike и ElectricalScooter нужно потребление энергии
            {
                output.PrintMessage("\nУточните тип транспорта:");
                output.PrintMessage("1. Электровелосипед (ElectricBike)");
                output.PrintMessage("2. Электросамокат (ElectricalScooter)");
                int? typeChoice = input.GetNumber("Выберите тип: ");
                int? consumption = input.GetNumber("Введите потребление энергии: ");

                if (typeChoice is null || consumption is null) break;

                if (typeChoice == 1)
                    newVehicle = new ElectricBike(name, id.Value, ease.Value, consumption.Value);
                else if (typeChoice == 2)
                    newVehicle = new ElectricalScooter(name, id.Value, ease.Value, consumption.Value);
                else
                    output.PrintMessage("Ошибка: Неизвестный тип транспорта.");
            }
            else if (energyChoice == 2) // Для обычного Bike потребление не требуется
            {
                newVehicle = new Bike(name, id.Value, ease.Value);
            }
            else
            {
                output.PrintMessage("Ошибка: Неизвестная категория.");
            }

            // Добавляем созданный транспорт в парк
            if (newVehicle is not null)
            {
                if (park.Add(newVehicle))
                    output.PrintMessage($"Транспорт '{newVehicle.Name}' успешно добавлен в парк!");
                else
                    output.PrintMessage("Отказ: транспорт не прошел техосмотр или ID уже занят.");
            }
            break;
        }

        case 2:
        {
            output.PrintMessage("\nВыберите категорию вещи:");
            output.PrintMessage("1. Энергопотребляющая (PowerBank, DocStation)");
            output.PrintMessage("2. Обычная (Helmet)");
            int? energyChoice = input.GetNumber("Выберите пункт меню: ");

            if (energyChoice is null) break;

            string name = input.GetLine("Введите название вещи: ");
            int? id = input.GetNumber("Введите инвентарный номер: ");

            if (id is null)
            {
                output.PrintMessage("Ошибка: некорректный ввод чисел.");
                break;
            }

            Thing? newThing = null;

            if (energyChoice == 1) // Для PowerBank и DocStation нужно потребление энергии
            {
                output.PrintMessage("\nУточните тип вещи:");
                output.PrintMessage("1. Внешний аккумулятор (PowerBank)");
                output.PrintMessage("2. Док-станция (DocStation)");
                int? typeChoice = input.GetNumber("Выберите тип: ");
                int? consumption = input.GetNumber("Введите потребление энергии: ");

                if (typeChoice is null || consumption is null) break;

                if (typeChoice == 1)
                    newThing = new PowerBank(name, id.Value, consumption.Value);
                else if (typeChoice == 2)
                    newThing = new DocStation(name, id.Value, consumption.Value);
                else
                    output.PrintMessage("Ошибка: Неизвестный тип вещи.");
            }
            else if (energyChoice == 2) // Для шлема энергия не нужна
            {
                newThing = new Helmet(name, id.Value);
            }
            else
            {
                output.PrintMessage("Ошибка: Неизвестная категория.");
            }

            // Добавляем вещь в парк
            if (newThing is not null)
            {
                if (park.Add(newThing))
                    output.PrintMessage($"Вещь '{newThing.Name}' успешно добавлена!");
                else
                    output.PrintMessage("Отказ: ID уже занят.");
            }
            break;
        }

        case 3:
            output.PrintAllInventory(park);
            break;

        case 4:
            output.PrintAllForBeginners(park);
            break;

        case 5:
            output.PrintMessage("--- СВОДКА ПО ПАРКУ ---");
            output.PrintCount(park);
            output.PrintConsumption(park);
            break;

        default:
            output.PrintMessage("Неизвестная команда. Попробуйте ещё раз.");
            break;
    }

}
