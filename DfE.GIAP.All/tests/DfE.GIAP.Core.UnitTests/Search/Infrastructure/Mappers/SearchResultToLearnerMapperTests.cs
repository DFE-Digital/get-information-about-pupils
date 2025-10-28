﻿using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Infrastructure.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.Mappers;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.Mappers;

public sealed class SearchResultToLearnerMapperTests
{
    private readonly IMapper<LearnerDataTransferObject, Learner> _searchResultToLearnerMapper;

    public SearchResultToLearnerMapperTests()
    {
        _searchResultToLearnerMapper = new SearchResultToLearnerMapper();
    }

    [Fact]
    public void Map_WithValidSearchResult_ReturnsConfiguredLearner()
    {
        // arrange
        LearnerDataTransferObject learnerDataTransferObject =
            LearnerDataTransferObjectTestDouble.Fake();

        // act
        Learner? result = _searchResultToLearnerMapper.Map(learnerDataTransferObject);

        // assert
        result.Should().NotBeNull();
        result.Identifier.UniqueLearnerNumber
            .Should().BeInRange(minimumValue: 1000000000, maximumValue: 2146999999);
        result.Name.Should().NotBeNull();
        result.Name.FirstName.Length.Should().BeGreaterThan(0);
        result.Characteristics.Should().NotBeNull();
        result.Characteristics.BirthDate.Should().NotBeNull();
        result.Characteristics.Sex.Should().BeOneOf(
            LearnerCharacteristics.Gender.Male,
            LearnerCharacteristics.Gender.Female,
            LearnerCharacteristics.Gender.Other);
        result.Identifier.Should().NotBeNull();
    }

    [Fact]
    public void Map_WithNullSearchResult_ThrowsExpectedArgumentNullException()
    {
        // act.
        LearnerDataTransferObject? learnerDataTransferObject = null!;

        // act.
        _searchResultToLearnerMapper
            .Invoking(mapper =>
                mapper.Map(learnerDataTransferObject))
                    .Should()
                        .Throw<ArgumentNullException>();
    }

    [Fact]
    public void Map_WithNullULN_ThrowsExpectedArgumentException()
    {
        // arrange
        LearnerDataTransferObject learnerDataTransferObject =
            LearnerDataTransferObjectTestDouble.Fake();

        learnerDataTransferObject.ULN = null!;

        // act, assert
        _searchResultToLearnerMapper
            .Invoking(mapper =>
                mapper.Map(learnerDataTransferObject))
                    .Should()
                        .Throw<ArgumentException>()
                        .WithMessage("Value cannot be null. (Parameter 'input.ULN')");
    }

    [Fact]
    public void Map_WithNullForename_ThrowsExpectedArgumentException()
    {
        // arrange
        LearnerDataTransferObject learnerDataTransferObject =
            LearnerDataTransferObjectTestDouble.Fake();

        learnerDataTransferObject.Forename = null!;

        // act, assert
        _searchResultToLearnerMapper
            .Invoking(mapper =>
                mapper.Map(learnerDataTransferObject))
                    .Should()
                        .Throw<ArgumentException>()
                        .WithMessage("Value cannot be null. (Parameter 'input.Forename')");
    }

    [Fact]
    public void Map_WithNullSurname_ThrowsExpectedArgumentException()
    {
        // arrange
        LearnerDataTransferObject learnerDataTransferObject =
            LearnerDataTransferObjectTestDouble.Fake();

        learnerDataTransferObject.Surname = null!;

        // act, assert
        _searchResultToLearnerMapper
            .Invoking(mapper =>
                mapper.Map(learnerDataTransferObject))
                    .Should()
                        .Throw<ArgumentException>()
                        .WithMessage("Value cannot be null. (Parameter 'input.Surname')");
    }

    [Fact]
    public void Map_WithNullDateOfBirth_ThrowsExpectedArgumentException()
    {
        // arrange
        LearnerDataTransferObject learnerDataTransferObject =
            LearnerDataTransferObjectTestDouble.Fake();

        learnerDataTransferObject.DOB = null!;

        // act, assert
        _searchResultToLearnerMapper
            .Invoking(mapper =>
                mapper.Map(learnerDataTransferObject))
                    .Should()
                        .Throw<ArgumentException>()
                        .WithMessage("Value cannot be null. (Parameter 'input.DOB')");
    }
}
