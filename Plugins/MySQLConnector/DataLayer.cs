﻿using System;
using System.Collections.Generic;
using Database;
using MySql.Data.MySqlClient;

namespace MySQLConnector
{
    internal class DataLayer : IDataLayer
    {
        private readonly MySqlConnector _database;

        public DataLayer(string name, MySqlConnector database)
        {
            Name = name;
            _database = database;
        }

        public string Name { get; }

        public void GetUser(string username, Action<IUser> callback)
        {
            var row = _database.ExecuteQuery(
                "SELECT ID, username, password FROM users WHERE username = @userName LIMIT 1;",
                new QueryParameter("@userName", MySqlDbType.VarChar, 60, "username", _database.EscapeString(username)));
            callback(new User(Convert.ToString(row[0]["username"]), Convert.ToString(row[0]["password"]), null, null, null));
        }

        public void UsernameAvailable(string username, Action<bool> callback)
        {
            var row = _database.ExecuteScalar(
                "SELECT ID FROM users WHERE username = @userName",
                new QueryParameter("@userName", MySqlDbType.VarChar, 60, "username", _database.EscapeString(username)));
            var isAvailable = row == null;
            callback(isAvailable);
        }

        public void AddNewUser(string username, string password, Action callback)
        {
            var row = _database.ExecuteNonQuery(
                "INSERT INTO users(username,password) VALUES(@userName,@pass)",
                new QueryParameter("@userName", MySqlDbType.VarChar, 60, "username", _database.EscapeString(username)),
                new QueryParameter("@pass", MySqlDbType.VarChar, 255, "password", _database.EscapeString(password)));
            callback();
        }

        public void DeleteUser(string username, Action callback)
        {
            var row = _database.ExecuteNonQuery(
                "DELETE FROM users WHERE username = @userName",
                new QueryParameter("@userName", MySqlDbType.VarChar, 60, "username", _database.EscapeString(username)));
            callback();
        }

        public void AddRequest(string sender, string receiver, Action callback)
        {
            var row = _database.ExecuteNonQuery(
                "INSERT INTO Friends(sender, request) " +
                "VALUES(@user, @receiver)",
                new QueryParameter("@user", MySqlDbType.VarChar, 60, "user", _database.EscapeString(sender)),
                new QueryParameter("@request", MySqlDbType.VarChar, 60, "request", _database.EscapeString(receiver)));
            callback();
        }

        public void RemoveRequest(string sender, string receiver, Action callback)
        {
            var row = _database.ExecuteNonQuery(
                "DELETE FROM Friends " +
                "WHERE(user = @sender AND request = @request)",
                new QueryParameter("@sender", MySqlDbType.VarChar, 60, "user", _database.EscapeString(sender)),
                new QueryParameter("@request", MySqlDbType.VarChar, 60, "request", _database.EscapeString(receiver)));
            callback();
        }

        public void AddFriend(string sender, string receiver, Action callback)
        {
            var row = _database.ExecuteNonQuery(
                "UPDATE Friends " +
                "SET friend = @friend, request = NULL " +
                "WHERE(user = @user AND request = @request)",
                new QueryParameter("@user", MySqlDbType.VarChar, 60, "user", _database.EscapeString(sender)),
                new QueryParameter("@friend", MySqlDbType.VarChar, 160, "friend", _database.EscapeString(receiver)),
                new QueryParameter("@request", MySqlDbType.VarChar, 60, "request", _database.EscapeString(receiver)));

            callback();
        }

        public void RemoveFriend(string sender, string receiver, Action callback)
        {
            var row = _database.ExecuteNonQuery(
                "DELETE FROM Friends " +
                "WHERE(user = @user AND friend = @friend)",
                new QueryParameter("@user", MySqlDbType.VarChar, 60, "user", _database.EscapeString(sender)),
                new QueryParameter("@friend", MySqlDbType.VarChar, 60, "friend", _database.EscapeString(receiver)));
            callback();
        }

        public void GetFriends(string username, Action<IFriendList> callback)
        {
            throw new NotImplementedException();
        }

        public void GetFriends(string username, Action<List<string>[]> callback)
        {
            var row = _database.ExecuteQuery(
                "SELECT user, friend, request FROM Friends WHERE user = @user OR request = @request;",
                new QueryParameter("@user", MySqlDbType.VarChar, 60, "user", _database.EscapeString(username)),
                new QueryParameter("@request", MySqlDbType.VarChar, 60, "request", _database.EscapeString(username)));


            var friends = new List<string>();
            var outRequests = new List<string>();
            var inRequests = new List<string>();

            foreach (var friend in row)
            {
                var relation = friend.GetRow();

                if (relation["user"].ToString() == username)
                {
                    if (relation["friend"].ToString() != null && relation["friend"].ToString() != "")
                    {
                        friends.Add(relation["friend"].ToString());
                    }
                    else
                    {
                        outRequests.Add(relation["request"].ToString());
                    }
                }
                else
                {
                    if (relation["request"].ToString() == username)
                    {
                        inRequests.Add(relation["user"].ToString());
                    }
                }
            }
            List<string>[] friendsLists = {friends, outRequests, inRequests};

            callback(friendsLists);
        }
    }
}