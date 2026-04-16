using System;
using System.Data;
using System.Net;
using System.Text.Json;
using Microsoft.VisualBasic;
using RestSharp;
using RestSharp.Authenticators;
using ExamPrep1.Models;

namespace ExamPrep1
{
    public class Tests
    {
        private RestClient client;
        private const string BaseUrl = "http://144.91.123.158:82";
        private const string StaticTocken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiJhMDgzNjA2Yi03OTE3LTQ2NjktYjk5Yy0yNzE5MmQwMWJiOTkiLCJpYXQiOiIwNC8xNi8yMDI2IDA3OjA0OjEzIiwiVXNlcklkIjoiZjhhN2Y4NTgtN2VlOC00ZmY0LTUzYWYtMDhkZTc2YTJkM2VjIiwiRW1haWwiOiJNZXJyeWJEMkBkbXguY29tIiwiVXNlck5hbWUiOiJtZXJyeVlCZCIsImV4cCI6MTc3NjM0NDY1MywiaXNzIjoiSWRlYUNlbnRlcl9BcHBfU29mdFVuaSIsImF1ZCI6IklkZWFDZW50ZXJfV2ViQVBJX1NvZnRVbmkifQ.wxtQyitOYRvfcZODpD4KW3Yw5EdVmnJpwsSVsqIovVU";
        private const string LoginEmail = "MerrybD2@dmx.com";
        private const string LoginPassword = "123456789";
        private static string lastCreatedIdeaId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticTocken))
            {
                jwtToken = StaticTocken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestRequest(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });
            var response = new RestClient(BaseUrl).Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }
        [Order(1)]
        [Test]
        public void CreateIdea_WithReqFields_shouldReturnSuccess()
        {
            var ideaData = new IdeaDTO
            {
                Title = "Test Idea",
                Description = "This is a test idea.",
                Url = ""
            };
            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(ideaData);

            var response = this.client.Execute(request);

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));
        }

        [Order(2)]
        [Test]
        public void GetAllIdeas_shouldReturnSuccess()
        {
            var request = new RestRequest("/api/Idea/All", Method.Get);
            var response = this.client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(responseItems, Is.Not.Null);
            Assert.That(responseItems, Is.Not.Empty);

            lastCreatedIdeaId = responseItems.LastOrDefault()?.Id;
        }


        [Order(3)]
        [Test]
        public void EditLastIdea_ShouldReturnSuccess()
        {
            var editedIdeaData = new IdeaDTO
            {
                Title = "Updated Test Idea",
                Description = "This is an updated test idea.",
                Url = ""
            };

            var request = new RestRequest($"/api/Idea/Edit", Method.Put);

            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            request.AddJsonBody(editedIdeaData);

            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"));
        }

        [Order(4)]
        [Test]
        public void DeleteLastIdea_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);           
            var response = this.client.Execute(request);

            //var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(response.Content, Is.EqualTo("\"The idea is deleted!\""));
        }

        [Order(5)]
        [Test]
        public void CreateIdeaWithoutRequeredFIelds_ShouldReturnBadRequest()
        {
            var ideaData = new IdeaDTO
            {
                Title = "",
                Description = "",
                Url = ""
            };
            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(ideaData);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
        }
        [Order(6)]
        [Test]
        public void EditIdeaWithInvalidId_ShouldReturnBadRequest()
        {
            string nonExistingId = "6666";
            var editedIdeaData = new IdeaDTO
            {
                Title = "Updated Test Idea",
                Description = "This is an updated test idea.",
                Url = ""
            };
            var request = new RestRequest($"/api/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaId", nonExistingId);
            request.AddJsonBody(editedIdeaData);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
            Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingIdea_ShouldReturnBadRequest()
        {
            string nonExistingId = "6666";
            var request = new RestRequest($"/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", nonExistingId);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
            Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
        }
        
        
        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}
