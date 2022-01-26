Feature: The page generation handler responds approriately to invalid inputs.

Background:
    * url apiHost

Scenario: Status 400 on non-JSON body.

    Given path 'CTS.Print', 'GenCache'
    And request 'foo = bar'
    When method post
    Then status 400
    And match response == 'Unable to parse request body.'

Scenario: Status 400 on broken JSON body.

    Given path 'CTS.Print', 'GenCache'
    And request '{"trial_ids": [ "NCI-2015-01906" '
    When method post
    Then status 400
    And match response == 'Unable to parse request body.'

Scenario: Status 400 when trial_ids is missing from the request body.

    Given path 'CTS.Print', 'GenCache'
    And request
    """
    {
        "link_template": "/about-cancer/treatment/clinical-trials/search/v?t=C3208&st=C3471&a=24&loc=1&z=20850&zp=100&rl=2&id=<TRIAL_ID>",
        "search_criteria": null
    }
    """
    When method post
    Then status 400
    And match reponse == 'trial_ids list is required.'

Scenario Outline: Status 400 when trial_ids is present but null or empty.

    Given path 'CTS.Print', 'GenCache'
    And request
    """
    {
        "trial_ids": trial_list
        "link_template": "/about-cancer/treatment/clinical-trials/search/v?t=C3208&st=C3471&a=24&loc=1&z=20850&zp=100&rl=2&id=<TRIAL_ID>",
        "search_criteria": null
    }
    """
    When method post
    Then status 400
    And match reponse == 'trial_ids list is required.'

    Examples:
    | trial_list |
    | []         |
    | null       |

