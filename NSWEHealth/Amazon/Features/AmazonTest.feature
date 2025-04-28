Feature: Test the Amazon Web App

Background: Open Amazon App
	Given I open the Amazon australia home page

Scenario: Verify Item Search
	When I want to search for 'Sony Tv'
	Then the result shows the list of the searched item

Scenario: Verify Filter selection of search result
	When I want to search for 'Sony Tv'
	And I select filter for 'Brand' as 'Sony'
	And I select filter for 'Resolution' as '4K'
	And I select filter for 'Model' as '2024'
	Then I verify all the filter checkboxes are checked

Scenario: Verify Sort by Price low to high of search result
	When I want to search for 'Sony Tv'
	And I select filter for 'Brand' as 'Sony'
	And I select filter for 'Resolution' as '4K'
	And I select filter for 'Model' as '2024'
	And I sort the result by price low to high
	Then items are sorted by the price from low to high