using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Contracts.User;

public record EnsureUserResponse(
    UserDto User,
    bool IsNew
);

