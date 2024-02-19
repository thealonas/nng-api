using Microsoft.AspNetCore.Mvc;

namespace nng_api.Helpers;

public static class OutputResult
{
    public static ObjectResult Ok(object data)
    {
        return new ObjectResult(data)
        {
            StatusCode = 200
        };
    }

    public static ObjectResult UserUpdated()
    {
        return new ObjectResult(new
        {
            code = 10,
            message = "User successfully updated"
        })
        {
            StatusCode = 200
        };
    }

    public static ObjectResult SettingsUpdated()
    {
        return new ObjectResult(new
        {
            code = 20,
            message = "Settings successfully updated"
        })
        {
            StatusCode = 200
        };
    }

    public static ObjectResult RequestUpdated()
    {
        return new ObjectResult(new
        {
            code = 30,
            message = "Request status successfully updated"
        })
        {
            StatusCode = 200
        };
    }

    public static ObjectResult WatchDogUpdated()
    {
        return new ObjectResult(new
        {
            code = 40,
            message = "Watchdog log successfully updated"
        })
        {
            StatusCode = 200
        };
    }

    public static ObjectResult RequestCreated()
    {
        return new ObjectResult(new
        {
            code = 201,
            message = "Request successfully created"
        })
        {
            StatusCode = 201
        };
    }

    public static ObjectResult GroupCreated()
    {
        return new ObjectResult(new
        {
            code = 201,
            message = "Group successfully created"
        })
        {
            StatusCode = 201
        };
    }

    public static ObjectResult UserCreated()
    {
        return new ObjectResult(new
        {
            code = 201,
            message = "User successfully created"
        })
        {
            StatusCode = 201
        };
    }

    public static ObjectResult NotAuthorized()
    {
        return new ObjectResult(new
        {
            code = 401,
            message = "Not authorized"
        })
        {
            StatusCode = 401
        };
    }

    public static ObjectResult InsufficientRights()
    {
        return new ObjectResult(new
        {
            code = 403,
            message = "Insufficient Rights"
        })
        {
            StatusCode = 403
        };
    }

    public static ObjectResult NotFound()
    {
        return new ObjectResult(new
        {
            code = 404,
            message = "Not found"
        })
        {
            StatusCode = 404
        };
    }

    public static ObjectResult NotFound(string customMessage)
    {
        return new ObjectResult(new
        {
            code = 404,
            message = customMessage
        })
        {
            StatusCode = 404
        };
    }

    public static ObjectResult InvalidInput()
    {
        return new ObjectResult(new
        {
            code = 405,
            message = "Invalid input"
        })
        {
            StatusCode = 405
        };
    }

    public static ObjectResult InvalidInput(string message)
    {
        return new ObjectResult(new
        {
            code = 405,
            message
        })
        {
            StatusCode = 405
        };
    }

    public static ObjectResult Conflict()
    {
        return new ObjectResult(new
        {
            code = 409,
            message = "Conflict"
        })
        {
            StatusCode = 409
        };
    }

    public static ObjectResult TeaPot(Exception e)
    {
        return new ObjectResult(new
        {
            code = 418,
            message = "An error occured",
            tea = e.FormatException()
        });
    }

    public static ObjectResult TeaPot(string message)
    {
        return new ObjectResult(new
        {
            code = 418,
            message = "An error occured",
            tea = message
        });
    }

    private static string FormatException(this Exception e)
    {
        return $"{e.GetType()}: {e.Message}";
    }
}
