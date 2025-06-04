using LdapHelper;

using Sandbox;

var connectionFactory = new LdapConnectionFactory(new LdapOptions()
{
    Host = "192.168.0.232",
    Zone = "local",
    Domain = "lorien",
    Server = "lorien.local",
    UserName = "Administrator",
    Password = "P@ssw0rd"
});

using var service = new ActiveDirectoryService(connectionFactory, new ActiveDirectoryOptions
{
    QueryBase = "DC=lorien,DC=local"
});

//var user = service.GetUserByUpn<ADUser>("Administrator@lorien.local", "CN=Users");
//var css = service.GetUserBySam<ADUser>("cal_sync_svc", "CN=Users");

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

//var users = service.GetUsers<ADUser>(ADUser.Attributes.ToArray(), "CN=Users");
//foreach (var user in users)
//{
//    Console.WriteLine(user.DistinguishedName);
//}

//var user = service.GetUserBySam<ADUser>("cal_sync_svc", "CN=Users");
//service.ChangePassword(user!.DistinguishedName, "pacan", "mazab");

var users = service.GetUsers<ADUser>(ADUser.Attributes.ToArray(), "(memberOf=CN=Tech,CN=Users,DC=lorien,DC=local)", "CN=Users");
foreach (var user in users)
{
    Console.WriteLine(user.Email);
}
