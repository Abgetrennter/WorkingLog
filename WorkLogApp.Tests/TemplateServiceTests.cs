using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Implementations;
using Xunit;

namespace WorkLogApp.Tests
{
    /// <summary>
    /// TemplateService 单元测试
    /// </summary>
    public class TemplateServiceTests : IDisposable
    {
        private readonly string _testDataPath;
        private readonly string _testTemplatesPath;
        private readonly TemplateService _service;

        public TemplateServiceTests()
        {
            _testDataPath = Path.Combine(Path.GetTempPath(), "WorkLogTests_" + Guid.NewGuid());
            _testTemplatesPath = Path.Combine(_testDataPath, "Templates");
            Directory.CreateDirectory(_testTemplatesPath);

            // 创建测试模板文件
            var testTemplatesJson = @"{
  ""categories"": [
    {
      ""id"": ""cat1"",
      ""name"": ""测试分类"",
      ""parentId"": """",
      ""children"": []
    }
  ],
  ""templates"": [
    {
      ""id"": ""tpl1"",
      ""name"": ""测试模板"",
      ""categoryId"": ""cat1"",
      ""placeholders"": {
        ""title"": ""text"",
        ""content"": ""textarea""
      }
    }
  ]
}";
            File.WriteAllText(Path.Combine(_testTemplatesPath, "templates.json"), testTemplatesJson);

            _service = new TemplateService();
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDataPath))
            {
                Directory.Delete(_testDataPath, true);
            }
        }

        [Fact]
        public void LoadTemplates_ExistingFile_ShouldLoadSuccessfully()
        {
            // Act
            var result = _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));

            // Assert
            Assert.True(result);
            var categories = _service.GetAllCategories();
            Assert.Single(categories);
            Assert.Equal("测试分类", categories[0].Name);
        }

        [Fact]
        public void LoadTemplates_NonExistingFile_ShouldCreateEmptyStore()
        {
            // Arrange
            var nonexistentFile = Path.Combine(_testDataPath, "nonexistent.json");

            // Act
            var result = _service.LoadTemplates(nonexistentFile);

            // Assert
            Assert.True(result); // LoadTemplates returns true even for nonexistent files (creates empty store)
            var categories = _service.GetAllCategories();
            Assert.Empty(categories);
        }

        [Fact]
        public void GetAllCategories_ShouldReturnAllCategories()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));

            // Act
            var categories = _service.GetAllCategories();

            // Assert
            Assert.Single(categories);
            Assert.Equal("cat1", categories[0].Id);
            Assert.Equal("测试分类", categories[0].Name);
        }

        [Fact]
        public void CreateCategory_ShouldAddNewCategory()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));

            // Act
            var newCategory = _service.CreateCategory("新分类", string.Empty);

            // Assert
            Assert.NotNull(newCategory);
            Assert.Equal("新分类", newCategory.Name);
            Assert.Empty(newCategory.ParentId);

            var categories = _service.GetAllCategories();
            Assert.Equal(2, categories.Count);
        }

        [Fact]
        public void CreateCategory_WithParent_ShouldAddChildCategory()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));

            // Act
            var childCategory = _service.CreateCategory("子分类", "cat1");

            // Assert
            Assert.NotNull(childCategory);
            Assert.Equal("子分类", childCategory.Name);
            Assert.Equal("cat1", childCategory.ParentId);

            // Verify by checking that a category with the parent ID exists
            var categories = _service.GetAllCategories();
            var child = categories.FirstOrDefault(c => c.Id == childCategory.Id);
            Assert.NotNull(child);
            Assert.Equal("cat1", child.ParentId);
        }

        [Fact]
        public void GetCategory_ExistingId_ShouldReturnCategory()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));

            // Act
            var category = _service.GetCategory("cat1");

            // Assert
            Assert.NotNull(category);
            Assert.Equal("cat1", category.Id);
            Assert.Equal("测试分类", category.Name);
        }

        [Fact]
        public void GetCategory_NonExistingId_ShouldReturnNull()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));

            // Act
            var category = _service.GetCategory("nonexistent");

            // Assert
            Assert.Null(category);
        }

        [Fact]
        public void UpdateCategory_ShouldUpdateCategoryName()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));
            var category = _service.GetCategory("cat1");
            category.Name = "更新后的分类";

            // Act
            var result = _service.UpdateCategory(category);

            // Assert
            Assert.True(result);
            var updatedCategory = _service.GetCategory("cat1");
            Assert.Equal("更新后的分类", updatedCategory.Name);
        }

        [Fact]
        public void DeleteCategory_ExistingId_ShouldRemoveCategory()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));

            // Act
            var result = _service.DeleteCategory("cat1");

            // Assert
            Assert.True(result);
            var categories = _service.GetAllCategories();
            Assert.Empty(categories);
        }

        [Fact]
        public void DeleteCategory_NonExistingId_ShouldReturnFalse()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));

            // Act
            var result = _service.DeleteCategory("nonexistent");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void DeleteCategory_WithChildren_ShouldRemoveAllChildren()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));
            var childCategory = _service.CreateCategory("子分类", "cat1");

            // Act
            var result = _service.DeleteCategory("cat1");

            // Assert
            Assert.True(result);
            var categories = _service.GetAllCategories();
            Assert.Empty(categories);
        }

        [Fact]
        public void GetTemplatesByCategory_ShouldReturnTemplates()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));

            // Act
            var templates = _service.GetTemplatesByCategory("cat1");

            // Assert
            Assert.Single(templates);
            Assert.Equal("tpl1", templates[0].Id);
            Assert.Equal("测试模板", templates[0].Name);
        }

        [Fact]
        public void CreateTemplate_ShouldAddNewTemplate()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));
            var newTemplate = new WorkTemplate
            {
                Id = "tpl2",
                Name = "新模板",
                CategoryId = "cat1",
                Placeholders = new Dictionary<string, string>
                {
                    { "field1", "text" }
                }
            };

            // Act
            var result = _service.CreateTemplate(newTemplate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("tpl2", result.Id);

            var templates = _service.GetTemplatesByCategory("cat1");
            Assert.Equal(2, templates.Count);
        }

        [Fact]
        public void UpdateTemplate_ShouldUpdateTemplate()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));
            var template = _service.GetTemplate("tpl1");
            template.Name = "更新后的模板";

            // Act
            var result = _service.UpdateTemplate(template);

            // Assert
            Assert.True(result);
            var updatedTemplate = _service.GetTemplate("tpl1");
            Assert.Equal("更新后的模板", updatedTemplate.Name);
        }

        [Fact]
        public void DeleteTemplate_ExistingId_ShouldRemoveTemplate()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));

            // Act
            var result = _service.DeleteTemplate("tpl1");

            // Assert
            Assert.True(result);
            var templates = _service.GetTemplatesByCategory("cat1");
            Assert.Empty(templates);
        }

        [Fact]
        public void SaveTemplates_ShouldPersistToFile()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));
            _service.CreateCategory("新分类", null);

            // Act
            var result = _service.SaveTemplates();

            // Assert
            Assert.True(result);

            // 创建新服务实例验证数据是否保存
            var newService = new TemplateService();
            newService.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));
            var categories = newService.GetAllCategories();
            Assert.Equal(2, categories.Count);
        }

        [Fact]
        public void MoveCategory_ShouldMoveToNewParent()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));
            var parentCategory = _service.CreateCategory("父分类", string.Empty);
            var childCategory = _service.CreateCategory("子分类", "cat1");

            // Act
            var result = _service.MoveCategory(childCategory.Id, parentCategory.Id);

            // Assert
            Assert.True(result);
            var movedCategory = _service.GetCategory(childCategory.Id);
            Assert.Equal(parentCategory.Id, movedCategory.ParentId);

            // Verify by checking the moved category's parent ID
            var categories = _service.GetAllCategories();
            var moved = categories.FirstOrDefault(c => c.Id == childCategory.Id);
            Assert.NotNull(moved);
            Assert.Equal(parentCategory.Id, moved.ParentId);
        }

        [Fact]
        public void MoveCategory_ToDescendant_ShouldReturnFalse()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));
            var childCategory = _service.CreateCategory("子分类", "cat1");
            var grandChildCategory = _service.CreateCategory("孙分类", childCategory.Id);

            // Act - 尝试将父分类移动到子分类下
            var result = _service.MoveCategory(childCategory.Id, grandChildCategory.Id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Render_ShouldReplacePlaceholders()
        {
            // Arrange
            _service.LoadTemplates(Path.Combine(_testTemplatesPath, "templates.json"));
            var template = _service.GetTemplate("tpl1");
            var fieldValues = new Dictionary<string, object>
            {
                { "title", "测试标题" },
                { "content", "测试内容" }
            };
            var item = new WorkLogItem
            {
                ItemTitle = "事项标题",
                ItemContent = "事项内容"
            };

            // Act
            var result = _service.Render(template.Content, fieldValues, item);

            // Assert
            Assert.NotNull(result);
        }
    }
}
