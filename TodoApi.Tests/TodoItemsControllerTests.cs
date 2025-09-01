using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using TodoApi.Controllers;
using TodoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace TodoApi.Tests
{
    public class TodoItemsControllerTests
    {
        private TodoContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new TodoContext(options);
        }

        public async Task GetTodoItems_ReturnsEmptyList_WhenNoItems()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var controller = new TodoItemsController(context);

            // Act
            var result = await controller.GetTodoItems();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TodoItem>>>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<TodoItem>>(actionResult.Value);
            Assert.Empty(items);
        }

        [Fact]
        public async Task PostTodoItem_CreatesTodoItem()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var controller = new TodoItemsController(context);
            var newItem = new TodoItem { Name = "測試項目", IsComplete = false };

            // Act
            var result = await controller.PostTodoItem(newItem);

            // Assert
            var actionResult = Assert.IsType<ActionResult<TodoItem>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnedItem = Assert.IsType<TodoItem>(createdAtActionResult.Value);

            Assert.Equal("測試項目", returnedItem.Name);
            Assert.False(returnedItem.IsComplete);
            Assert.True(returnedItem.Id > 0);
        }

        [Fact]
        public async Task GetTodoItem_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var controller = new TodoItemsController(context);

            // Act
            var result = await controller.GetTodoItem(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetTodoItem_ReturnsTodoItem_WhenItemExists()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var todoItem = new TodoItem { Name = "現有項目", IsComplete = false };
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            var controller = new TodoItemsController(context);

            // Act
            var result = await controller.GetTodoItem(todoItem.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<TodoItem>>(result);
            var returnedItem = Assert.IsType<TodoItem>(actionResult.Value);
            Assert.Equal(todoItem.Id, returnedItem.Id);
            Assert.Equal("現有項目", returnedItem.Name);
        }
    }
}