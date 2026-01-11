using LdapHelper;

using Sandbox;

var ldapOptions = new LdapOptions()
{
    Host = "192.168.0.241",
    Zone = "local",
    Domain = "lorien",
    Server = "lorien.local",
    Username = "Administrator",
    Password = "P@ssw0rd"
};

var connectionFactory = new LdapConnectionFactory(ldapOptions);

using var connection = connectionFactory.CreateConnection();

var service = new ActiveDirectoryService(connection);

// Query base: DC=lorien,DC=local

FilterBuilder johnFilter = new FilterBuilder()
    .AddObjectCategory()
    .AddObjectClass()
    .AddAccountEnabledFilter()
    .AddUpn("john@lorien.local");

var john = service.GetUsers<User>(User.Attributes, "OU=RNB Users,DC=lorien,DC=local", johnFilter.Build())
    .SingleOrDefault();

if (john != null)
{
    Console.WriteLine($"John found: {john.Name}");
}

//var css = service.GetUserBySam<User>("cal_sync_svc", "CN=Users");

//if (user is not null)
//{
//    Console.WriteLine($"""
//        Got a user!
//        And his name is -> {user.DistinguishedName}
//        """);
//}
//else
//{
//    Console.WriteLine("User not found.");
//}

//var users = service.GetUsers<User>(User.Attributes.ToArray(), "CN=Users");
//foreach (var user in users)
//{
//    Console.WriteLine(user.DistinguishedName);
//}

//var user = service.GetUserBySam<User>("cal_sync_svc", "CN=Users");
//service.ChangePassword(user!.DistinguishedName, "pacan", "mazab");

FilterBuilder usersFilter = new FilterBuilder()
    .AddObjectCategory()
    .AddObjectClass()
    .AddAccountEnabledFilter()
    .AddMemberOf("CN=DealerManagers,OU=RNB Groups,DC=lorien,DC=local");

var users = service.GetUsers<User>(User.Attributes, "OU=RNB Users,DC=lorien,DC=local", usersFilter.Build());
foreach (var user in users)
{
    Console.WriteLine(user.UserPrincipalName);
}
