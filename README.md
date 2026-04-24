# SGL Script

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/license-MIT-green)
![Status](https://img.shields.io/badge/status-active%20prototype-0ea5e9)

Минималистичный интерпретируемый язык для игровой логики и алгоритмических сценариев.

SGL Script заточен под простую и читаемую запись правил: условия, циклы, массивы, диапазоны, базовая математика и встроенные функции. Проект написан на C# и состоит из классического пайплайна `Lexer -> Parser -> AST -> Interpreter`.

## Зачем этот проект

- Площадка для экспериментов с дизайном DSL и интерпретаторами.
- Компактная реализация языка, которую легко читать и расширять.
- Наглядные примеры алгоритмов: факториал, простые числа, сортировка.

## Возможности языка

- Типы: `number`, `string`, `bool`, `array`, `object`.
- Операторы: `+`, `-`, `*`, `/`, `%`, `>`, `<`, `=`, `and`, `or`, `not`.
- Управление потоком: `if / else`, `for ... in ...`, `break`, `return`.
- Диапазоны: `1..5`, `a..b`.
- Массивы: литералы, объединение/вычитание, методы коллекций.
- Встроенные функции: `Print`, `IsNumber`, `IsString`, `IsArray`, `IsBool`, `Sqrt`, `Floor`.
- Ввод аргументов через `with`:
	`with n: number, s: string, arr: array`.

## Пример SGL-кода

```sgl
with n: number

if n < 2
		Print 1
}

factorial = 1
for i in 2..n
		factorial = factorial * i
}

Print factorial
```

## Быстрый старт

Требования:

- .NET SDK 9.0+

Запуск скрипта через `dotnet run`:

```bash
dotnet run --project .\Sgl-script\Sgl-script.csproj .\factorial.sgl 6
```

Ожидаемый вывод:

```text
720
```

Запуск готового `exe` (Windows):

```bat
run.bat factorial.sgl 6
```

## Примеры в репозитории

- `sample.sgl` - обзор синтаксиса и возможностей.
- `factorial.sgl` - вычисление факториала.
- `primes.sgl` - решето простых чисел.
- `sort.sgl` - сортировка массива.

## Архитектура

- `Lexer.cs` - токенизация исходного текста.
- `Parser.cs` - построение AST.
- `Ast.cs` - узлы синтаксического дерева.
- `Interpreter.cs` - выполнение программы.
- `ExecutionContext.cs` - регистрация функций/методов и аргументы запуска.
- `MemoryManager.cs` - области видимости и хранение переменных.

## Лицензия

Проект распространяется под лицензией MIT. См. `LICENSE`.
