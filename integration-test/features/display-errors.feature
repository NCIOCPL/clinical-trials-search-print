Feature: The display print handler responds approriately to invalid inputs.

    Background:
        * url apiHost

    Scenario: Status 400 when there is no print ID.

        Given path 'CTS.Print', 'Display'
        When method get
        Then status 400
        And match response == 'Invalid printid'


    Scenario Outline: Status 400 when the print ID is not a valid GUID.
        bad_id: 'bad_id'

        Given path 'CTS.Print', 'Display'
        And param printid = bad_id
        When method get
        Then status 400
        And match response == 'Invalid printid'

        Examples:
            | bad_id |
            # Not even close
            | chicken |
            # Missing digits (too short)
            | 1B83536F-2F5D-EC11-82F8 |


    Scenario: Status 404 for an unknown print ID.

        Given path 'CTS.Print', 'Display'
        And param printid = '00000000-0000-0000-0000-000000000005'
        When method get
        Then status 404
        And match response == 'Not Found'
