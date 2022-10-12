Feature: Convertor

A short summary of the feature

@FeaturesTesting
Scenario: I give a link to the Convertor api and ask for a json
	Given I am using the Convertor api
	When The URI is a link to a csv file
	Then I receive a json or a xml as a string


